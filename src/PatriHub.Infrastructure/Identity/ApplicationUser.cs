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

    /// <summary>
    /// Quando a senha foi trocada pela última vez via /api/auth/redefinir-senha — `null` pra
    /// quem nunca passou por esse fluxo (inclui todo mundo registrado antes dele existir).
    /// Usado só para invalidar sessões antigas (ver <see cref="VerificadorSenhaAlterada"/>
    /// e ADR-0009); não é atualizado no registro nem em nenhum outro fluxo de troca de senha,
    /// porque não existe outro hoje.
    /// </summary>
    public DateTimeOffset? SenhaAlteradaEm { get; set; }
}
