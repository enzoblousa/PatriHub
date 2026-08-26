namespace PatriHub.Infrastructure.Identity;

/// <summary>
/// Credenciais do primeiro Admin, seedado no startup (ver IdentitySeeder.SeedAdminAsync) — sem
/// isso não existe nenhum jeito de uma conta virar Admin pela API (registro sempre cria papel
/// User). `Email`/`Senha` ausentes ou vazios (padrão em ambiente de teste) desativam o seed.
/// </summary>
public sealed class AdminBootstrapOptions
{
    public const string SectionName = "AdminBootstrap";

    public string? Email { get; init; }

    public string? Senha { get; init; }
}
