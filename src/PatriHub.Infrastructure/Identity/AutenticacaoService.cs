using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatriHub.Application.Autenticacao;
using PatriHub.Application.Common;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Email;
using PatriHub.Infrastructure.Jwt;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Identity;

public sealed class AutenticacaoService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator,
    PatriHubDbContext db,
    IEnviadorDeEmail enviadorDeEmail,
    IConfiguration configuration,
    ILogger<AutenticacaoService> logger) : IAutenticacaoService
{
    public async Task<ResultadoAutenticacao> RegistrarAsync(RegistrarUsuarioRequest request)
    {
        Usuario usuario;
        try
        {
            usuario = Usuario.Registrar(request.Nome, request.Email, request.ConsentimentoLgpd);
        }
        catch (ArgumentException ex)
        {
            return ResultadoAutenticacao.ComErro(ex.Message);
        }

        var existente = await userManager.FindByEmailAsync(usuario.Email);
        if (existente is not null)
        {
            return ResultadoAutenticacao.ComErro("Já existe uma conta com este email.");
        }

        var applicationUser = new ApplicationUser
        {
            Id = usuario.Id,
            UserName = usuario.Email,
            Email = usuario.Email,
            Nome = usuario.Nome,
            CriadoEm = usuario.CriadoEm,
            ConsentimentoLgpdEm = usuario.ConsentimentoLgpdEm,
        };

        var criado = await userManager.CreateAsync(applicationUser, request.Senha);
        if (!criado.Succeeded)
        {
            var erro = string.Join("; ", criado.Errors.Select(e => e.Description));
            return ResultadoAutenticacao.ComErro(erro);
        }

        await userManager.AddToRoleAsync(applicationUser, usuario.Papel.ToString());

        return GerarResultado(applicationUser, [usuario.Papel.ToString()]);
    }

    public async Task<ResultadoAutenticacao> LoginAsync(LoginRequest request)
    {
        var applicationUser = await userManager.FindByEmailAsync(Usuario.NormalizarEmail(request.Email));
        if (applicationUser is null || !await userManager.CheckPasswordAsync(applicationUser, request.Senha))
        {
            return ResultadoAutenticacao.ComErro("Email ou senha inválidos.");
        }

        // Conta desativada pelo Admin (ver AdminService.AtualizarStatusUsuarioAsync) — mesma
        // mensagem genérica de "Email ou senha inválidos" seria enganosa aqui, então é
        // reportada à parte.
        if (await userManager.IsLockedOutAsync(applicationUser))
        {
            return ResultadoAutenticacao.ComErro("Conta desativada. Entre em contato com o suporte.");
        }

        var papeis = await userManager.GetRolesAsync(applicationUser);
        return GerarResultado(applicationUser, papeis);
    }

    /// <summary>
    /// "Esqueci minha senha" (ver ADR-0009). Decisão consciente de revelar quando o email não
    /// existe (<see cref="TipoErroOperacao.NaoEncontrado"/>) em vez da mensagem genérica de
    /// "se esse email existir..." — trade-off de UX aceito apesar de permitir enumerar contas.
    /// Falha no envio do email em si (Resend fora do ar etc.) não vira erro aqui — ver
    /// <see cref="ResendEnviadorDeEmail"/> — pra não vazar detalhe de infra pro cliente.
    /// </summary>
    public async Task<ResultadoOperacao> SolicitarRecuperacaoSenhaAsync(SolicitarRecuperacaoSenhaRequest request)
    {
        var applicationUser = await userManager.FindByEmailAsync(Usuario.NormalizarEmail(request.Email));
        if (applicationUser is null)
        {
            return ResultadoOperacao.ComErro("Não existe conta com este email.", TipoErroOperacao.NaoEncontrado);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(applicationUser);

        var frontendBaseUrl = configuration["Frontend:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend:BaseUrl não configurado.");
        var link = $"{frontendBaseUrl}/redefinir-senha" +
            $"?email={Uri.EscapeDataString(applicationUser.Email!)}&token={Uri.EscapeDataString(token)}";

        await enviadorDeEmail.EnviarRecuperacaoSenhaAsync(applicationUser.Email!, applicationUser.Nome, link);

        return ResultadoOperacao.ComSucesso();
    }

    /// <summary>
    /// Conclui a recuperação de senha — ver ADR-0009. `ResetPasswordAsync` já valida o token
    /// (assinatura, expiração — 30min, ver DependencyInjection — e uso único) e as regras de
    /// força da nova senha, então os dois tipos de falha chegam pelo mesmo `IdentityResult` e
    /// viram a mesma <see cref="TipoErroOperacao.Validacao"/>: não há necessidade (nem seria
    /// seguro) de distinguir "token inválido" de "senha fraca" pro chamador aqui. Em caso de
    /// sucesso, marca `SenhaAlteradaEm` pra invalidar qualquer JWT emitido antes disso (ver
    /// VerificadorSenhaAlterada).
    /// </summary>
    public async Task<ResultadoOperacao> RedefinirSenhaAsync(RedefinirSenhaRequest request)
    {
        var applicationUser = await userManager.FindByEmailAsync(Usuario.NormalizarEmail(request.Email));
        if (applicationUser is null)
        {
            // Mesma mensagem de token inválido do Identity (ver IdentityErrorDescriberPtBr) —
            // não faz sentido distinguir "email não existe" de "token inválido" aqui: quem
            // chega até aqui só veio de um link de email que o próprio backend gerou.
            return ResultadoOperacao.ComErro("Token inválido.", TipoErroOperacao.Validacao);
        }

        var resultado = await userManager.ResetPasswordAsync(applicationUser, request.Token, request.NovaSenha);
        if (!resultado.Succeeded)
        {
            var erro = string.Join("; ", resultado.Errors.Select(e => e.Description));
            return ResultadoOperacao.ComErro(erro, TipoErroOperacao.Validacao);
        }

        applicationUser.SenhaAlteradaEm = DateTimeOffset.UtcNow;
        var atualizado = await userManager.UpdateAsync(applicationUser);
        if (!atualizado.Succeeded)
        {
            // A senha já trocou de verdade nesse ponto — só o timestamp de invalidação de
            // sessão que não gravou. Logado, não reportado como falha ao cliente: a pior
            // consequência é sessões antigas continuarem válidas até expirar naturalmente
            // (mesmo comportamento de antes desta feature existir), não perda de dado.
            logger.LogError(
                "Falha ao gravar SenhaAlteradaEm para o usuário {UsuarioId} após reset de senha bem-sucedido.",
                applicationUser.Id);
        }

        return ResultadoOperacao.ComSucesso();
    }

    /// <summary>
    /// Hard delete da conta e de todo o histórico financeiro (ver ADR-0005): sem
    /// anonimização, sem soft delete — os dados somem de verdade. Ordem de exclusão segue as
    /// FKs `Restrict` do modelo (PatriHubDbContext): Lancamentos antes de Contratos/Ativos,
    /// Contratos antes de Ativos/Locatarios. Tudo numa transação com a remoção do
    /// ApplicationUser, pra nunca sobrar histórico órfão se a exclusão do usuário no Identity
    /// falhar.
    /// </summary>
    public async Task<ResultadoOperacao> ExcluirContaAsync(Guid usuarioId)
    {
        var applicationUser = await userManager.FindByIdAsync(usuarioId.ToString());
        if (applicationUser is null)
        {
            return ResultadoOperacao.ComErro("Usuário não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        await using var transacao = await db.Database.BeginTransactionAsync();

        var lancamentos = await db.Lancamentos.Where(l => l.UsuarioId == usuarioId).ToListAsync();
        db.Lancamentos.RemoveRange(lancamentos);

        var contratos = await db.Contratos.Where(c => c.UsuarioId == usuarioId).ToListAsync();
        db.Contratos.RemoveRange(contratos);

        // Query em Ativos (não num DbSet<Imovel>/DbSet<Carro> específico) materializa o tipo
        // concreto (Imovel/Carro) via TPT — Remove/SaveChanges já apaga a linha da tabela
        // derivada junto com a de Ativos, sem SQL manual pra cada tabela.
        var ativos = await db.Ativos.Where(a => a.UsuarioId == usuarioId).ToListAsync();
        db.Ativos.RemoveRange(ativos);

        var locatarios = await db.Locatarios.Where(l => l.UsuarioId == usuarioId).ToListAsync();
        db.Locatarios.RemoveRange(locatarios);

        await db.SaveChangesAsync();

        var excluido = await userManager.DeleteAsync(applicationUser);
        if (!excluido.Succeeded)
        {
            await transacao.RollbackAsync();
            var erro = string.Join("; ", excluido.Errors.Select(e => e.Description));
            return ResultadoOperacao.ComErro(erro, TipoErroOperacao.Validacao);
        }

        await transacao.CommitAsync();
        return ResultadoOperacao.ComSucesso();
    }

    private ResultadoAutenticacao GerarResultado(ApplicationUser applicationUser, IEnumerable<string> papeis)
    {
        var (token, expiraEm) = jwtTokenGenerator.GerarToken(applicationUser, papeis);
        var papel = papeis.FirstOrDefault() ?? PapelUsuario.User.ToString();

        var usuarioDto = new UsuarioDto(applicationUser.Id, applicationUser.Nome, applicationUser.Email!, papel);
        return ResultadoAutenticacao.ComSucesso(token, expiraEm, usuarioDto);
    }
}
