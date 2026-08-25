using Microsoft.AspNetCore.Identity;
using PatriHub.Domain.Entidades;

namespace PatriHub.Infrastructure.Identity;

/// <summary>
/// Representação de persistência do Usuario via ASP.NET Core Identity. O papel (User/Admin)
/// vive nas Roles do Identity; Nome/CriadoEm são específicos do domínio do PatriHub.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string Nome { get; set; } = string.Empty;

    public DateTimeOffset CriadoEm { get; set; }

    public PapelUsuario Papel { get; set; } = PapelUsuario.User;
}
