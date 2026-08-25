using Microsoft.AspNetCore.Identity;
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
}
