using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PatriHub.Application.Autenticacao;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Identity;

namespace PatriHub.Api.IntegrationTests;

/// <summary>Registra um usuário de teste e devolve um HttpClient já autenticado com o Bearer token.</summary>
public static class AutenticacaoTestHelper
{
    public const string SenhaPadrao = "SenhaForte123!";

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
            new RegistrarUsuarioRequest(nome ?? "Maria Silva", EmailUnico(), SenhaPadrao, ConsentimentoLgpd: true));
        var resultado = await registro.Content.ReadFromJsonAsync<ResultadoAutenticacao>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resultado!.Token);
        return (client, resultado.Usuario!.Id);
    }

    /// <summary>
    /// Registra um usuário, eleva pra Role Admin diretamente via UserManager (não existe
    /// self-service de virar Admin — ver AC da issue #8) e reloga, pois o JWT emitido no
    /// registro já saiu sem a claim de Role Admin. Devolve um cliente autenticado com esse token.
    /// </summary>
    public static async Task<(HttpClient Client, Guid UsuarioId)> CriarClienteAdminAutenticadoComIdAsync(this PatriHubApiFactory factory, string? nome = null)
    {
        var email = EmailUnico();
        var clienteRegistro = factory.CreateClient();
        var registro = await clienteRegistro.PostAsJsonAsync(
            "/api/auth/registrar",
            new RegistrarUsuarioRequest(nome ?? "Admin Suporte", email, SenhaPadrao, ConsentimentoLgpd: true));
        var resultadoRegistro = await registro.Content.ReadFromJsonAsync<ResultadoAutenticacao>();
        var usuarioId = resultadoRegistro!.Usuario!.Id;

        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var applicationUser = await userManager.FindByIdAsync(usuarioId.ToString());
            await userManager.AddToRoleAsync(applicationUser!, PapelUsuario.Admin.ToString());
        }

        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, SenhaPadrao));
        var resultadoLogin = await login.Content.ReadFromJsonAsync<ResultadoAutenticacao>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resultadoLogin!.Token);
        return (client, usuarioId);
    }

    public static async Task<HttpClient> CriarClienteAdminAutenticadoAsync(this PatriHubApiFactory factory, string? nome = null)
    {
        var (client, _) = await factory.CriarClienteAdminAutenticadoComIdAsync(nome);
        return client;
    }
}
