namespace PatriHub.Infrastructure.Jwt;

/// <summary>
/// Sem refresh token no MVP — ver ADR-0001. Vida longa (~7 dias) evita relogin frequente.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Secret { get; init; }

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public int ExpiraEmDias { get; init; } = 7;
}
