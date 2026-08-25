using System.Net;
using System.Net.Http.Json;
using PatriHub.Application.Locatarios;

namespace PatriHub.Api.IntegrationTests;

public sealed class LocatariosTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
{
    private static LocatarioRequest LocatarioValido(string nome = "João Souza") =>
        new(nome, "123.456.789-09", "(11) 99999-0000", "joao@example.com");

    [Fact]
    public async Task Criar_locatario_com_dados_validos_retorna_201_com_detalhe()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var response = await client.PostAsJsonAsync("/api/locatarios", LocatarioValido());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<LocatarioDto>();
        Assert.NotNull(dto);
        Assert.Equal("João Souza", dto!.Nome);
        Assert.Equal("12345678909", dto.Cpf);
    }

    [Fact]
    public async Task Criar_locatario_com_CPF_invalido_retorna_400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var response = await client.PostAsJsonAsync("/api/locatarios", LocatarioValido() with { Cpf = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Atualizar_locatario_troca_dados()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var criado = await (await client.PostAsJsonAsync("/api/locatarios", LocatarioValido())).Content.ReadFromJsonAsync<LocatarioDto>();

        var response = await client.PutAsJsonAsync($"/api/locatarios/{criado!.Id}", LocatarioValido("Maria Lima"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atualizado = await response.Content.ReadFromJsonAsync<LocatarioDto>();
        Assert.Equal("Maria Lima", atualizado!.Nome);
    }

    [Fact]
    public async Task Atualizar_locatario_de_outro_usuario_retorna_404()
    {
        var dono = await factory.CriarClienteAutenticadoAsync();
        var criado = await (await dono.PostAsJsonAsync("/api/locatarios", LocatarioValido())).Content.ReadFromJsonAsync<LocatarioDto>();

        var outroUsuario = await factory.CriarClienteAutenticadoAsync();
        var response = await outroUsuario.PutAsJsonAsync($"/api/locatarios/{criado!.Id}", LocatarioValido("Maria Lima"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Listar_retorna_apenas_locatarios_do_usuario_autenticado()
    {
        var usuario1 = await factory.CriarClienteAutenticadoAsync();
        var usuario2 = await factory.CriarClienteAutenticadoAsync();

        await usuario1.PostAsJsonAsync("/api/locatarios", LocatarioValido("Locatário 1"));
        await usuario2.PostAsJsonAsync("/api/locatarios", LocatarioValido("Locatário 2"));

        var listagem = await usuario1.GetFromJsonAsync<List<LocatarioDto>>("/api/locatarios");

        var resultado = Assert.Single(listagem!);
        Assert.Equal("Locatário 1", resultado.Nome);
    }

    [Fact]
    public async Task Chamar_endpoints_de_locatarios_sem_token_retorna_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/locatarios");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
