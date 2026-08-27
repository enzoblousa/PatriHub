using Microsoft.AspNetCore.Identity;

namespace PatriHub.Infrastructure.Identity;

/// <summary>
/// Representação de persistência do Usuario via ASP.NET Core Identity. O papel (User/Admin)
/// vive só nas Roles do Identity (ver <see cref="AutenticacaoService"/>) — nenhuma cópia
/// própria aqui, pra não ter duas fontes de verdade pro mesmo dado.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string Nome { get; set; } = string.Empty;

    public DateTimeOffset CriadoEm { get; set; }

    /// <summary>Timestamp do aceite da Política de Privacidade no registro — ver Usuario.Registrar.</summary>
    public DateTimeOffset ConsentimentoLgpdEm { get; set; }
}
