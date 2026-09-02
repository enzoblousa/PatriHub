using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PatriHub.Application.Ativos;
using PatriHub.Application.Autenticacao;
using PatriHub.Application.Contratos;
using PatriHub.Application.Lancamentos;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Identity;

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
        Assert.Contains("aceitar o uso dos dados", corpo);
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

    /// <summary>Simula o link do email: gera o token do jeito que SolicitarRecuperacaoSenhaAsync gera, sem passar pelo envio de email de verdade (ver EnviadorDeEmailConsole, escolhido nos testes por não haver Resend:ApiKey configurada).</summary>
    private async Task<string> GerarTokenDeResetAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var usuario = await userManager.FindByEmailAsync(email);
        return await userManager.GeneratePasswordResetTokenAsync(usuario!);
    }

    [Fact]
    public async Task EsqueciSenha_com_email_existente_retorna_200()
    {
        var email = EmailUnico();
        await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true));

        var response = await _client.PostAsJsonAsync("/api/auth/esqueci-senha", new SolicitarRecuperacaoSenhaRequest(email));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>Q3 da recuperação de senha (ADR-0009): decisão consciente de revelar quando o email não existe, em vez da mensagem genérica.</summary>
    [Fact]
    public async Task EsqueciSenha_com_email_inexistente_retorna_404()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/esqueci-senha", new SolicitarRecuperacaoSenhaRequest(EmailUnico()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Não existe conta", corpo);
    }

    [Fact]
    public async Task RedefinirSenha_com_token_valido_troca_a_senha_e_permite_login_com_a_nova()
    {
        var email = EmailUnico();
        await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true));
        var token = await GerarTokenDeResetAsync(email);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/redefinir-senha",
            new RedefinirSenhaRequest(email, token, "SenhaNova456!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginComSenhaAntiga = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "SenhaForte123!"));
        Assert.Equal(HttpStatusCode.Unauthorized, loginComSenhaAntiga.StatusCode);

        var loginComSenhaNova = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "SenhaNova456!"));
        Assert.Equal(HttpStatusCode.OK, loginComSenhaNova.StatusCode);
    }

    [Fact]
    public async Task RedefinirSenha_com_token_invalido_retorna_400()
    {
        var email = EmailUnico();
        await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true));

        var response = await _client.PostAsJsonAsync(
            "/api/auth/redefinir-senha",
            new RedefinirSenhaRequest(email, "token-invalido", "SenhaNova456!"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Q4/Q6 da recuperação de senha (ADR-0009): redefinir a senha invalida sessões JWT emitidas
    /// antes da troca, mesmo sem refresh token/sessão server-side (ver SessaoInvalidadaMiddleware).
    /// Não checa o token antes da troca (como os outros testes de "me" fazem) de propósito: isso
    /// aqueceria o cache de 60s de VerificadorSenhaAlterada com `SenhaAlteradaEm = null` pra esse
    /// usuário, e a assertiva de baixo poderia ler esse valor obsoleto em vez de ir ao banco de
    /// novo — o teste ficaria instável (racy) sem exercitar o middleware de verdade.
    /// </summary>
    [Fact]
    public async Task RedefinirSenha_invalida_o_token_JWT_emitido_antes_da_troca()
    {
        var email = EmailUnico();
        var registro = await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Maria Silva", email, "SenhaForte123!", ConsentimentoLgpd: true));
        var resultadoRegistro = await registro.Content.ReadFromJsonAsync<ResultadoAutenticacao>();

        var clienteComTokenAntigo = factory.CreateClient();
        clienteComTokenAntigo.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resultadoRegistro!.Token);

        var token = await GerarTokenDeResetAsync(email);
        await _client.PostAsJsonAsync("/api/auth/redefinir-senha", new RedefinirSenhaRequest(email, token, "SenhaNova456!"));

        var response = await clienteComTokenAntigo.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
