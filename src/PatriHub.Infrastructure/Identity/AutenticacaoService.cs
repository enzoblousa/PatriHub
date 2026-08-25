using Microsoft.AspNetCore.Identity;
using PatriHub.Application.Autenticacao;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Jwt;

namespace PatriHub.Infrastructure.Identity;

public sealed class AutenticacaoService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator) : IAutenticacaoService
{
    public async Task<ResultadoAutenticacao> RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken ct = default)
    {
        Usuario usuario;
        try
        {
            usuario = Usuario.Registrar(request.Nome, request.Email);
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
            Papel = usuario.Papel,
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

    public async Task<ResultadoAutenticacao> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var applicationUser = await userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (applicationUser is null || !await userManager.CheckPasswordAsync(applicationUser, request.Senha))
        {
            return ResultadoAutenticacao.ComErro("Email ou senha inválidos.");
        }

        var papeis = await userManager.GetRolesAsync(applicationUser);
        return GerarResultado(applicationUser, papeis);
    }

    private ResultadoAutenticacao GerarResultado(ApplicationUser applicationUser, IEnumerable<string> papeis)
    {
        var (token, expiraEm) = jwtTokenGenerator.GerarToken(applicationUser, papeis);
        var papel = papeis.FirstOrDefault() ?? PapelUsuario.User.ToString();

        var usuarioDto = new UsuarioDto(applicationUser.Id, applicationUser.Nome, applicationUser.Email!, papel);
        return ResultadoAutenticacao.ComSucesso(token, expiraEm, usuarioDto);
    }
}
