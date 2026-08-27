using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PatriHub.Application.Autenticacao;
using PatriHub.Application.Common;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Jwt;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Identity;

public sealed class AutenticacaoService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator,
    PatriHubDbContext db) : IAutenticacaoService
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
