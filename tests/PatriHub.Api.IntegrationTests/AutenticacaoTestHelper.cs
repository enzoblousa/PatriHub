using System.Net.Http.Headers;
using System.Net.Http.Json;
using PatriHub.Application.Autenticacao;

namespace PatriHub.Api.IntegrationTests;

/// <summary>Registra um usuário de teste e devolve um HttpClient já autenticado com o Bearer token.</summary>
public static class AutenticacaoTestHelper
{
    public static string EmailUnico() => $"usuario-{Guid.NewGuid():N}@example.com";

    public static async Task<HttpClient> CriarClienteAutenticadoAsync(this PatriHubApiFactory factory, string? nome = null)
    {
        var (client, _) = await factory.CriarClienteAutenticadoComIdAsync(nome);
        return client;
    }

    /// <summary>Igual a <see cref="CriarClienteAutenticadoAsync"/>, mas também devolve o `UsuarioId` — usado por testes que resolvem um serviço diretamente do container de DI (ver DashboardTests).</summary>
    public static async Task<(HttpClient Client, Guid UsuarioId)> CriarClienteAutenticadoComIdAsync(this PatriHubApiFactory factory, string? nome = null)
    {
        var client = factory.CreateClient();
        var registro = await client.PostAsJsonAsync(
            "/api/auth/registrar",
            new RegistrarUsuarioRequest(nome ?? "Maria Silva", EmailUnico(), "SenhaForte123!"));
        var resultado = await registro.Content.ReadFromJsonAsync<ResultadoAutenticacao>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resultado!.Token);
        return (client, resultado.Usuario!.Id);
    }
}
