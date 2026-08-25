namespace PatriHub.Infrastructure.Jwt;

/// <summary>
/// Claims customizadas do PatriHub no JWT, além das padrão (sub, email, role). Uma única
/// definição, usada tanto por quem emite (JwtTokenGenerator) quanto por quem lê (AuthController).
/// </summary>
public static class PatriHubClaimTypes
{
    public const string Nome = "nome";
}
