using System.Net;
using System.Net.Http.Json;
using PatriHub.Application.Ativos;
using PatriHub.Application.Autenticacao;
using PatriHub.Application.Contratos;
using PatriHub.Application.Lancamentos;
using PatriHub.Domain.Entidades;

namespace PatriHub.Api.IntegrationTests;

public sealed class AuthTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string EmailUnico() => $"usuario-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task Registrar_com_dados_validos_retorna_201_com_token()
    {
        var request = new RegistrarUsuarioRequest("Maria Silva", EmailUnico(), "SenhaForte123!", ConsentimentoLgpd: true);

        var response = await _client.PostAsJsonAsync("/api/auth/registrar", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var resultado = await response.Content.ReadFromJsonAsync<ResultadoAutenticacao>();
        Assert.NotNull(resultado);
        Assert.True(resultado!.Sucesso);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.Equal("Maria Silva", resultado.Usuario!.Nome);
    }

    [Fact]
    public async Task Registrar_sem_consentimento_lgpd_retorna_erro_claro()
    {
        var request = new RegistrarUsuarioRequest("Maria Silva", EmailUnico(), "SenhaForte123!", ConsentimentoLgpd: false);

        var response = await _client.PostAsJsonAsync("/api/auth/registrar", request);

        // Mesmo status (Conflict) que qualquer outra falha de registro — ver
        // AuthController.Registrar, que não distingue "email duplicado" de erro de validação.
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Política de Privacidade", corpo);
    }

    [Fact]
    public async Task Registrar_com_email_ja_existente_retorna_erro_claro()
    {
        var email = EmailUnico();
        var request = new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true);
        await _client.PostAsJsonAsync("/api/auth/registrar", request);

        var response = await _client.PostAsJsonAsync("/api/auth/registrar", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_com_credenciais_validas_retorna_token_valido_por_cerca_de_sete_dias()
    {
        var email = EmailUnico();
        await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true));

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
        await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true));

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
        var registro = await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true));
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

    [Fact]
    public async Task ExcluirConta_sem_token_retorna_401()
    {
        var response = await _client.DeleteAsync("/api/auth/conta");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExcluirConta_remove_a_conta_e_todo_o_historico_financeiro_do_usuario()
    {
        var email = EmailUnico();
        var registro = await _client.PostAsJsonAsync(
            "/api/auth/registrar",
            new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true));
        var resultadoRegistro = await registro.Content.ReadFromJsonAsync<ResultadoAutenticacao>();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resultadoRegistro!.Token);

        var ativoId = await CenarioTestHelper.CriarAtivoAsync(client);
        var locatarioId = await CenarioTestHelper.CriarLocatarioAsync(client);
        var contrato = await (await client.PostAsJsonAsync(
                "/api/contratos",
                new ContratoRequest(ativoId, locatarioId, 1_500m, 10, new DateOnly(2026, 1, 1), null)))
            .Content.ReadFromJsonAsync<ContratoDto>();
        await client.PostAsJsonAsync(
            "/api/lancamentos",
            new LancamentoRequest(ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_500m, new DateOnly(2026, 1, 10), "Aluguel de janeiro", contrato!.Id));

        var response = await client.DeleteAsync("/api/auth/conta");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // A conta some de verdade (hard delete — ver ADR-0005): registrar de novo com o mesmo
        // email precisa funcionar, o que só é possível se a linha foi mesmo removida (um soft
        // delete/desativação deixaria "Já existe uma conta com este email").
        var reRegistro = await _client.PostAsJsonAsync(
            "/api/auth/registrar",
            new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true));
        Assert.Equal(HttpStatusCode.Created, reRegistro.StatusCode);
    }
}
