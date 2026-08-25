using System.Net;
using System.Net.Http.Json;
using PatriHub.Application.Ativos;
using PatriHub.Domain.Entidades;

namespace PatriHub.Api.IntegrationTests;

public sealed class AtivosTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
{
    private static EnderecoDto EnderecoValido() =>
        new("Rua das Flores", "123", null, "Centro", "São Paulo", "SP", "01000-000");

    private static ImovelRequest ImovelValido(string apelido = "Apê Centro") => new(
        apelido,
        new DateOnly(2020, 1, 10),
        ValorAquisicao: 300_000m,
        ValorMercadoAtual: 350_000m,
        EnderecoValido(),
        TipoImovel.Apartamento,
        AreaM2: 65m,
        Matricula: "12345",
        ValorIptuMensal: 150m,
        ValorCondominioMensal: 400m,
        Financiamento: null);

    private static CarroRequest CarroValido(string apelido = "Corolla") => new(
        apelido,
        new DateOnly(2022, 3, 15),
        ValorAquisicao: 120_000m,
        ValorMercadoAtual: 100_000m,
        Placa: "ABC1D23",
        Marca: "Toyota",
        Modelo: "Corolla",
        AnoFabricacao: 2022,
        AnoModelo: 2022,
        ValorFipeAtual: 105_000m,
        Km: 30_000m,
        ConsumoMedio: 14.5m,
        Financiamento: null);

    [Fact]
    public async Task Criar_imovel_com_dados_validos_retorna_201_com_detalhe()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var response = await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var detalhe = await response.Content.ReadFromJsonAsync<AtivoDetalheDto>();
        Assert.NotNull(detalhe);
        Assert.Equal(TipoAtivo.Imovel, detalhe!.Tipo);
        Assert.Equal(StatusAtivo.Vago, detalhe.Status);
        Assert.NotNull(detalhe.Imovel);
        Assert.Equal("12345", detalhe.Imovel!.Matricula);
        Assert.Null(detalhe.Carro);
    }

    [Fact]
    public async Task Criar_carro_com_dados_validos_retorna_201_com_detalhe()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var response = await client.PostAsJsonAsync("/api/ativos/carros", CarroValido());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var detalhe = await response.Content.ReadFromJsonAsync<AtivoDetalheDto>();
        Assert.NotNull(detalhe);
        Assert.Equal(TipoAtivo.Carro, detalhe!.Tipo);
        Assert.NotNull(detalhe.Carro);
        Assert.Equal("ABC1D23", detalhe.Carro!.Placa);
        Assert.Null(detalhe.Imovel);
    }

    [Fact]
    public async Task Criar_imovel_com_apelido_vazio_retorna_400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var response = await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido(apelido: ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Atualizar_imovel_troca_ValorMercadoAtual()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var criado = await (await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido())).Content.ReadFromJsonAsync<AtivoDetalheDto>();

        var atualizarRequest = ImovelValido("Apê Paulista") with { ValorMercadoAtual = 420_000m };

        var response = await client.PutAsJsonAsync($"/api/ativos/imoveis/{criado!.Id}", atualizarRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atualizado = await response.Content.ReadFromJsonAsync<AtivoDetalheDto>();
        Assert.Equal(420_000m, atualizado!.ValorMercadoAtual);
        Assert.Equal("Apê Paulista", atualizado.Apelido);
    }

    [Fact]
    public async Task Atualizar_carro_de_outro_usuario_retorna_404()
    {
        var dono = await factory.CriarClienteAutenticadoAsync();
        var criado = await (await dono.PostAsJsonAsync("/api/ativos/carros", CarroValido())).Content.ReadFromJsonAsync<AtivoDetalheDto>();

        var outroUsuario = await factory.CriarClienteAutenticadoAsync();
        var response = await outroUsuario.PutAsJsonAsync($"/api/ativos/carros/{criado!.Id}", CarroValido() with { ValorMercadoAtual = 1m });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarcarStatus_com_Manutencao_altera_o_status_do_ativo()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var criado = await (await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido())).Content.ReadFromJsonAsync<AtivoDetalheDto>();

        var response = await client.PatchAsJsonAsync($"/api/ativos/{criado!.Id}/status", new MarcarStatusAtivoRequest(StatusAtivo.Manutencao));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atualizado = await response.Content.ReadFromJsonAsync<AtivoDetalheDto>();
        Assert.Equal(StatusAtivo.Manutencao, atualizado!.Status);
    }

    [Fact]
    public async Task MarcarStatus_com_Alugado_retorna_400_pois_e_automatico()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var criado = await (await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido())).Content.ReadFromJsonAsync<AtivoDetalheDto>();

        var response = await client.PatchAsJsonAsync($"/api/ativos/{criado!.Id}/status", new MarcarStatusAtivoRequest(StatusAtivo.Alugado));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Excluir_ativo_faz_sumir_da_listagem_mas_detalhe_direto_retorna_404()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var criado = await (await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido())).Content.ReadFromJsonAsync<AtivoDetalheDto>();

        var exclusao = await client.DeleteAsync($"/api/ativos/{criado!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, exclusao.StatusCode);

        var detalhe = await client.GetAsync($"/api/ativos/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, detalhe.StatusCode);

        var listagem = await client.GetFromJsonAsync<List<AtivoResumoDto>>("/api/ativos");
        Assert.DoesNotContain(listagem!, a => a.Id == criado.Id);
    }

    [Fact]
    public async Task Listar_retorna_apenas_ativos_do_usuario_autenticado_com_lucro_do_mes_zero()
    {
        var usuario1 = await factory.CriarClienteAutenticadoAsync();
        var usuario2 = await factory.CriarClienteAutenticadoAsync();

        var criadoUsuario1 = await (await usuario1.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido())).Content.ReadFromJsonAsync<AtivoDetalheDto>();
        await usuario2.PostAsJsonAsync("/api/ativos/carros", CarroValido());

        var listagem = await usuario1.GetFromJsonAsync<List<AtivoResumoDto>>("/api/ativos");

        var resumo = Assert.Single(listagem!);
        Assert.Equal(criadoUsuario1!.Id, resumo.Id);
        Assert.Equal(0m, resumo.LucroDoMes);
    }

    [Fact]
    public async Task ObterDetalhe_de_ativo_de_outro_usuario_retorna_404()
    {
        var dono = await factory.CriarClienteAutenticadoAsync();
        var criado = await (await dono.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido())).Content.ReadFromJsonAsync<AtivoDetalheDto>();

        var outroUsuario = await factory.CriarClienteAutenticadoAsync();
        var response = await outroUsuario.GetAsync($"/api/ativos/{criado!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Chamar_endpoints_de_ativos_sem_token_retorna_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/ativos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
