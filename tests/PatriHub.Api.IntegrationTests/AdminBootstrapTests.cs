using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PatriHub.Application.Autenticacao;

namespace PatriHub.Api.IntegrationTests;

/// <summary>
/// Cobre IdentitySeeder.SeedAdminAsync, disparado no startup a partir de
/// `AdminBootstrap:Email`/`AdminBootstrap:Senha` (ver appsettings.json) — sem esse seed não
/// existiria nenhum jeito de uma conta virar Admin pela API (registro público sempre cria User).
/// </summary>
public sealed class AdminBootstrapTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
{
    private const string Email = "admin@patrihub.local";
    private const string Senha = "Dev-only-senha-troque-me-123!";

    [Fact]
    public async Task Conta_seedada_via_AdminBootstrap_consegue_logar_com_papel_Admin()
    {
        var login = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(Email, Senha));

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var resultado = await login.Content.ReadFromJsonAsync<ResultadoAutenticacao>();
        Assert.Equal("Admin", resultado!.Usuario!.Papel);
    }

    [Fact]
    public async Task Conta_seedada_via_AdminBootstrap_acessa_rotas_de_admin()
    {
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(Email, Senha));
        var resultado = await login.Content.ReadFromJsonAsync<ResultadoAutenticacao>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resultado!.Token);

        var response = await client.GetAsync("/api/admin/usuarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
