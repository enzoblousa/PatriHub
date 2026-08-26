using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PatriHub.Application.Admin;
using PatriHub.Application.Ativos;
using PatriHub.Application.Common;
using PatriHub.Application.Lancamentos;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Identity;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Admin;

/// <summary>
/// Toda leitura de Ativos/Lançamentos de outro usuário é delegada aos serviços existentes
/// (<see cref="IAtivoService"/>/<see cref="ILancamentoService"/>), que já filtram por
/// `UsuarioId` — aqui só se troca o `usuarioId` do dono da conta autenticada pelo
/// `usuarioAlvoId` da conta sob suporte, e grava a auditoria (ver ADR-0002).
/// </summary>
public sealed class AdminService(
    PatriHubDbContext db,
    UserManager<ApplicationUser> userManager,
    IAtivoService ativoService,
    ILancamentoService lancamentoService) : IAdminService
{
    public async Task<IReadOnlyList<UsuarioAdminDto>> ListarUsuariosAsync(Guid adminUsuarioId)
    {
        var usuarios = await userManager.Users.OrderBy(u => u.Email).ToListAsync();

        // Um único SELECT com join em vez de userManager.GetRolesAsync por usuário — evita
        // 1 query por linha da listagem.
        var papeisPorUsuario = (await db.UserRoles
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .ToListAsync())
            .ToLookup(x => x.UserId, x => x.Name);

        var resultado = new List<UsuarioAdminDto>(usuarios.Count);
        foreach (var usuario in usuarios)
        {
            var ativo = !await userManager.IsLockedOutAsync(usuario);
            var papel = papeisPorUsuario[usuario.Id].FirstOrDefault() ?? PapelUsuario.User.ToString();
            resultado.Add(new UsuarioAdminDto(usuario.Id, usuario.Nome, usuario.Email!, papel!, ativo, usuario.CriadoEm));

            // Nome/email/status de conta de outro usuário também é dado pessoal sob LGPD — audita
            // um registro por usuário retornado (nunca para o próprio Admin), mesmo sendo uma
            // listagem (ver ADR-0002: "todo acesso do Admin a dado de outro usuário").
            await RegistrarAuditoriaAsync(adminUsuarioId, usuario.Id, RecursoAuditoria.Usuario, usuario.Id);
        }

        return resultado;
    }

    public async Task<ResultadoOperacao> AtualizarStatusUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId, bool ativo)
    {
        if (adminUsuarioId == usuarioAlvoId)
        {
            return ResultadoOperacao.ComErro("Não é possível ativar/desativar a própria conta.", TipoErroOperacao.Validacao);
        }

        var usuarioAlvo = await userManager.FindByIdAsync(usuarioAlvoId.ToString());
        if (usuarioAlvo is null)
        {
            return ResultadoOperacao.ComErro("Usuário não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        // LockoutEnabled precisa ser true para IsLockedOutAsync respeitar o LockoutEnd (o
        // Identity só verifica a data quando o lockout está habilitado pro usuário) — setado
        // aqui de forma defensiva, independente do que a conta já tinha.
        await userManager.SetLockoutEnabledAsync(usuarioAlvo, true);
        await userManager.SetLockoutEndDateAsync(usuarioAlvo, ativo ? null : DateTimeOffset.MaxValue);

        await RegistrarAuditoriaAsync(adminUsuarioId, usuarioAlvoId, RecursoAuditoria.Usuario, usuarioAlvoId);
        return ResultadoOperacao.ComSucesso();
    }

    /// <summary>
    /// Sem checagem de `adminUsuarioId == usuarioAlvoId` aqui (diferente de
    /// <see cref="AtualizarStatusUsuarioAsync"/>): não existe hoje nenhum endpoint de
    /// autoatendimento pra trocar a própria senha, então bloquear o Admin de resetar a própria
    /// senha por aqui o deixaria sem nenhuma forma de trocá-la.
    /// </summary>
    public async Task<ResultadoOperacao> ResetarSenhaAsync(Guid adminUsuarioId, Guid usuarioAlvoId, string novaSenha)
    {
        var usuarioAlvo = await userManager.FindByIdAsync(usuarioAlvoId.ToString());
        if (usuarioAlvo is null)
        {
            return ResultadoOperacao.ComErro("Usuário não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        // Sem GeneratePasswordResetTokenAsync/ResetPasswordAsync aqui: aquele fluxo existe pro
        // caso de "esqueci minha senha" anônimo, com token expirável enviado por email — o Admin
        // já está autenticado e autorizado (role Admin no JWT), então troca a senha direto.
        // Valida a nova senha ANTES de remover a antiga: RemovePasswordAsync/AddPasswordAsync não
        // são atômicos, e uma novaSenha inválida não pode deixar a conta sem senha nenhuma.
        var validacao = await ValidarSenhaAsync(usuarioAlvo, novaSenha);
        if (ParaErro(validacao) is { } erroValidacao)
        {
            return erroValidacao;
        }

        var removido = await userManager.RemovePasswordAsync(usuarioAlvo);
        if (ParaErro(removido) is { } erroRemocao)
        {
            return erroRemocao;
        }

        var adicionado = await userManager.AddPasswordAsync(usuarioAlvo, novaSenha);
        if (ParaErro(adicionado) is { } erroAdicao)
        {
            return erroAdicao;
        }

        await RegistrarAuditoriaAsync(adminUsuarioId, usuarioAlvoId, RecursoAuditoria.Usuario, usuarioAlvoId);
        return ResultadoOperacao.ComSucesso();
    }

    public async Task<ResultadoOperacao<IReadOnlyList<AtivoResumoDto>>> ListarAtivosDoUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId)
    {
        if (!await UsuarioExisteAsync(usuarioAlvoId))
        {
            return ResultadoOperacao<IReadOnlyList<AtivoResumoDto>>.ComErro("Usuário não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        var ativos = await ativoService.ListarAsync(usuarioAlvoId);
        await RegistrarAuditoriaAsync(adminUsuarioId, usuarioAlvoId, RecursoAuditoria.Ativos);
        return ResultadoOperacao<IReadOnlyList<AtivoResumoDto>>.ComSucesso(ativos);
    }

    /// <summary>
    /// Sem <see cref="UsuarioExisteAsync"/> aqui (diferente de <see cref="ListarAtivosDoUsuarioAsync"/>):
    /// <see cref="IAtivoService.ObterDetalheAsync"/> já devolve NaoEncontrado quando o Ativo não
    /// existe pra `usuarioAlvoId`, o que cobre tanto "Ativo inexistente" quanto "usuário
    /// inexistente" com o mesmo 404 — a checagem só é necessária na listagem, que nunca falha
    /// (uma lista vazia seria ambígua entre "sem Ativos" e "usuário não existe").
    /// </summary>
    public async Task<ResultadoOperacao<AtivoDetalheDto>> ObterAtivoDoUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId, Guid ativoId)
    {
        var resultado = await ativoService.ObterDetalheAsync(usuarioAlvoId, ativoId);
        if (!resultado.Sucesso)
        {
            return resultado;
        }

        await RegistrarAuditoriaAsync(adminUsuarioId, usuarioAlvoId, RecursoAuditoria.Ativos, ativoId);
        return resultado;
    }

    public async Task<ResultadoOperacao<IReadOnlyList<LancamentoDto>>> ListarLancamentosDoUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId, LancamentoFiltro filtro)
    {
        if (!await UsuarioExisteAsync(usuarioAlvoId))
        {
            return ResultadoOperacao<IReadOnlyList<LancamentoDto>>.ComErro("Usuário não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        var lancamentos = await lancamentoService.ListarAsync(usuarioAlvoId, filtro);
        await RegistrarAuditoriaAsync(adminUsuarioId, usuarioAlvoId, RecursoAuditoria.Lancamentos);
        return ResultadoOperacao<IReadOnlyList<LancamentoDto>>.ComSucesso(lancamentos);
    }

    public async Task<ResultadoOperacao<LancamentoDto>> ObterLancamentoDoUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId, Guid lancamentoId)
    {
        var resultado = await lancamentoService.ObterDetalheAsync(usuarioAlvoId, lancamentoId);
        if (!resultado.Sucesso)
        {
            return resultado;
        }

        await RegistrarAuditoriaAsync(adminUsuarioId, usuarioAlvoId, RecursoAuditoria.Lancamentos, lancamentoId);
        return resultado;
    }

    private Task<bool> UsuarioExisteAsync(Guid usuarioId) =>
        db.Users.AnyAsync(u => u.Id == usuarioId);

    /// <summary>Roda os mesmos <see cref="UserManager{TUser}.PasswordValidators"/> que CreateAsync/ResetPasswordAsync usariam, sem exigir o token provider daquele fluxo.</summary>
    private async Task<IdentityResult> ValidarSenhaAsync(ApplicationUser usuario, string novaSenha)
    {
        foreach (var validador in userManager.PasswordValidators)
        {
            var resultado = await validador.ValidateAsync(userManager, usuario, novaSenha);
            if (!resultado.Succeeded)
            {
                return resultado;
            }
        }

        return IdentityResult.Success;
    }

    /// <summary>Mapeia um <see cref="IdentityResult"/> falho pro mesmo formato de erro do resto do serviço — <c>null</c> quando `resultado.Succeeded`.</summary>
    private static ResultadoOperacao? ParaErro(IdentityResult resultado) =>
        resultado.Succeeded
            ? null
            : ResultadoOperacao.ComErro(string.Join("; ", resultado.Errors.Select(e => e.Description)), TipoErroOperacao.Validacao);

    /// <summary>
    /// Nunca grava quando `adminUsuarioId == usuarioAlvoId` — auditoria existe só para acesso a
    /// dado de OUTRO usuário (ver ADR-0002, <see cref="AuditLogAdmin.Registrar"/>).
    /// </summary>
    private async Task RegistrarAuditoriaAsync(Guid adminUsuarioId, Guid usuarioAlvoId, RecursoAuditoria recurso, Guid? recursoId = null)
    {
        if (adminUsuarioId == usuarioAlvoId)
        {
            return;
        }

        db.AuditLogsAdmin.Add(AuditLogAdmin.Registrar(adminUsuarioId, usuarioAlvoId, recurso, recursoId));
        await db.SaveChangesAsync();
    }
}
