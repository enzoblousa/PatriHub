using System.Net;
using System.Net.Http.Json;
using PatriHub.Application.Autenticacao;

namespace PatriHub.Api.IntegrationTests;

public sealed class AuthTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string EmailUnico() => $"usuario-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task Registrar_com_dados_validos_retorna_201_com_token()
    {
        var request = new RegistrarUsuarioRequest("Maria Silva", EmailUnico(), "SenhaForte123!");

        var response = await _client.PostAsJsonAsync("/api/auth/registrar", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var resultado = await response.Content.ReadFromJsonAsync<ResultadoAutenticacao>();
        Assert.NotNull(resultado);
        Assert.True(resultado!.Sucesso);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.Equal("Maria Silva", resultado.Usuario!.Nome);
    }

    [Fact]
    public async Task Registrar_com_email_ja_existente_retorna_erro_claro()
    {
        var email = EmailUnico();
        var request = new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!");
        await _client.PostAsJsonAsync("/api/auth/registrar", request);

        var response = await _client.PostAsJsonAsync("/api/auth/registrar", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_com_credenciais_validas_retorna_token_valido_por_cerca_de_sete_dias()
    {
        var email = EmailUnico();
        await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "SenhaForte123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultado = await response.Content.ReadFromJsonAsync<ResultadoAutenticacao>();
        Assert.NotNull(resultado);
        Assert.True(resultado!.Sucesso);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        var diasAteExpirar = (resultado.ExpiraEm!.Value - DateTimeOffset.UtcNow).TotalDays;
        Assert.InRange(diasAteExpirar, 6.9, 7.1);
    }

    [Fact]
    public async Task Login_com_senha_incorreta_retorna_401()
    {
        var email = EmailUnico();
        await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "SenhaErrada123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Chamar_endpoint_protegido_sem_token_retorna_401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_com_token_valido_retorna_os_dados_do_usuario_autenticado()
    {
        var email = EmailUnico();
        var registro = await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!"));
        var resultadoRegistro = await registro.Content.ReadFromJsonAsync<ResultadoAutenticacao>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resultadoRegistro!.Token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var usuario = await response.Content.ReadFromJsonAsync<UsuarioDto>();
        Assert.NotNull(usuario);
        Assert.Equal(email, usuario!.Email);
        Assert.Equal("Maria Silva", usuario.Nome);
        Assert.Equal("User", usuario.Papel);
    }
}
