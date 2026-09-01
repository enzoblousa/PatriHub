using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace PatriHub.Api.IntegrationTests;

/// <summary>
/// Seam 2: sobe a API real (WebApplicationFactory) contra um Postgres real (Testcontainers),
/// aplicando as migrations do EF Core — nada de mock de banco.
/// </summary>
public sealed class PatriHubApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("patrihub")
        .WithUsername("patrihub")
        .WithPassword("patrihub")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PatriHubDb"] = _postgres.GetConnectionString(),
                // Testes não precisam (nem devem pagar o custo de) a massa de dados demo —
                // cada teste cria só os dados que precisa via CenarioTestHelper/AutenticacaoTestHelper.
                ["SeedDadosDemo"] = "false",
                // O rate limiter de /api/auth/* (ver Program.cs) particiona por IP, e todo request
                // in-memory do WebApplicationFactory cai no mesmo loopback — sem isso, o volume de
                // chamadas a registrar/login feito por AutenticacaoTestHelper em toda a suíte
                // estouraria o limite de produção (5/60s) bem antes de terminar.
                ["RateLimiting:Auth:PermitLimit"] = "1000000",
                ["RateLimiting:Auth:WindowSeconds"] = "1",
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Acessar Services aqui força o host a subir agora (com o container já no ar), o que
        // por sua vez roda as migrations e o seed de Roles definidos em Program.cs.
        _ = Services;
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
