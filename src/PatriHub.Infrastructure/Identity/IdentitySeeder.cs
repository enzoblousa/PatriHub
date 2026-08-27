using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatriHub.Domain.Entidades;

namespace PatriHub.Infrastructure.Identity;

/// <summary>
/// Garante que as Roles do Identity (User/Admin) existam antes de qualquer registro/login.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var papel in Enum.GetValues<PapelUsuario>())
        {
            var nomePapel = papel.ToString();
            if (!await roleManager.RoleExistsAsync(nomePapel))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(nomePapel));
            }
        }
    }

    /// <summary>
    /// Cria a primeira conta Admin a partir de `AdminBootstrap:Email`/`AdminBootstrap:Senha` (ver
    /// <see cref="AdminBootstrapOptions"/>) — o registro público (AutenticacaoService.RegistrarAsync)
    /// sempre cria papel User, então sem isso não haveria nenhum jeito de ter um Admin. Idempotente:
    /// roda em todo startup, mas nunca sobrescreve senha de uma conta já existente, só garante o
    /// papel Admin nela.
    /// </summary>
    public static async Task SeedAdminAsync(IServiceProvider services, IConfiguration configuration)
    {
        var opcoes = configuration.GetSection(AdminBootstrapOptions.SectionName).Get<AdminBootstrapOptions>();
        if (string.IsNullOrWhiteSpace(opcoes?.Email) || string.IsNullOrWhiteSpace(opcoes.Senha))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var email = Usuario.NormalizarEmail(opcoes.Email);
        var existente = await userManager.FindByEmailAsync(email);
        if (existente is not null)
        {
            if (!await userManager.IsInRoleAsync(existente, PapelUsuario.Admin.ToString()))
            {
                await userManager.AddToRoleAsync(existente, PapelUsuario.Admin.ToString());
            }

            return;
        }

        // consentimentoLgpd: true aqui não representa um aceite de usuário real — é uma conta
        // provisionada via configuração de infraestrutura (AdminBootstrap), não um cadastro
        // pelo formulário público sujeito ao consentimento LGPD (ver Usuario.Registrar).
        var usuario = Usuario.Registrar("Admin PatriHub", email, consentimentoLgpd: true, papel: PapelUsuario.Admin);
        var applicationUser = new ApplicationUser
        {
            Id = usuario.Id,
            UserName = usuario.Email,
            Email = usuario.Email,
            Nome = usuario.Nome,
            CriadoEm = usuario.CriadoEm,
            ConsentimentoLgpdEm = usuario.ConsentimentoLgpdEm,
        };

        // Falha aqui (ex.: AdminBootstrap:Senha não atende a política de senha) tem que travar o
        // startup, não passar em silêncio — do contrário o ambiente sobe sem nenhuma conta Admin
        // e ninguém percebe até precisar de uma.
        var criado = await userManager.CreateAsync(applicationUser, opcoes.Senha);
        if (!criado.Succeeded)
        {
            var erro = string.Join("; ", criado.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Falha ao seedar a conta AdminBootstrap ({email}): {erro}");
        }

        await userManager.AddToRoleAsync(applicationUser, PapelUsuario.Admin.ToString());
    }
}
