using System.Net;
using System.Net.Http.Json;
using PatriHub.Application.Ativos;
using PatriHub.Application.Contratos;
using PatriHub.Application.Lancamentos;
using PatriHub.Application.Locatarios;
using PatriHub.Domain.Entidades;

namespace PatriHub.Api.IntegrationTests;

public sealed class ContratosTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
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

    private static LocatarioRequest LocatarioValido(string nome = "João Souza") =>
        new(nome, "123.456.789-09", "(11) 99999-0000", "joao@example.com");

    private static async Task<Guid> CriarAtivoAsync(HttpClient client, string apelido = "Apê Centro")
    {
        var criado = await (await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido(apelido))).Content.ReadFromJsonAsync<AtivoDetalheDto>();
        return criado!.Id;
    }

    private static async Task<Guid> CriarLocatarioAsync(HttpClient client, string nome = "João Souza")
    {
        var criado = await (await client.PostAsJsonAsync("/api/locatarios", LocatarioValido(nome))).Content.ReadFromJsonAsync<LocatarioDto>();
        return criado!.Id;
    }

    private static ContratoRequest ContratoValido(Guid ativoId, Guid locatarioId) => new(
        ativoId,
        locatarioId,
        ValorAluguelMensal: 1_500m,
        DiaVencimento: 10,
        DataInicio: new DateOnly(2026, 1, 1),
        DataFim: null);

    private static async Task<StatusAtivo> StatusDoAtivoAsync(HttpClient client, Guid ativoId)
    {
        var detalhe = await client.GetFromJsonAsync<AtivoDetalheDto>($"/api/ativos/{ativoId}");
        return detalhe!.Status;
    }

    [Fact]
    public async Task Criar_contrato_com_dados_validos_retorna_201_e_muda_status_do_ativo_para_Alugado()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);

        var response = await client.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatarioId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ContratoDto>();
        Assert.NotNull(dto);
        Assert.Equal(StatusContrato.Ativo, dto!.Status);
        Assert.Equal(StatusAtivo.Alugado, await StatusDoAtivoAsync(client, ativoId));
    }

    [Fact]
    public async Task Criar_segundo_contrato_Ativo_para_o_mesmo_ativo_retorna_400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatario1 = await CriarLocatarioAsync(client, "Locatário 1");
        var locatario2 = await CriarLocatarioAsync(client, "Locatário 2");
        await client.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatario1));

        var response = await client.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatario2));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_contrato_para_ativo_de_outro_usuario_retorna_404()
    {
        var dono = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(dono);

        var outroUsuario = await factory.CriarClienteAutenticadoAsync();
        var locatarioId = await CriarLocatarioAsync(outroUsuario);
        var response = await outroUsuario.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatarioId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Criar_contrato_com_locatario_de_outro_usuario_retorna_404()
    {
        var usuario1 = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(usuario1);

        var usuario2 = await factory.CriarClienteAutenticadoAsync();
        var locatarioDeOutroUsuario = await CriarLocatarioAsync(usuario2);

        var response = await usuario1.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatarioDeOutroUsuario));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Encerrar_contrato_muda_status_para_Encerrado_e_ativo_volta_a_Vago()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        var criado = await (await client.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatarioId))).Content.ReadFromJsonAsync<ContratoDto>();

        var response = await client.PostAsync($"/api/contratos/{criado!.Id}/encerrar", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var encerrado = await response.Content.ReadFromJsonAsync<ContratoDto>();
        Assert.Equal(StatusContrato.Encerrado, encerrado!.Status);
        Assert.Equal(StatusAtivo.Vago, await StatusDoAtivoAsync(client, ativoId));
    }

    [Fact]
    public async Task Encerrar_contrato_ja_encerrado_retorna_400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        var criado = await (await client.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatarioId))).Content.ReadFromJsonAsync<ContratoDto>();
        await client.PostAsync($"/api/contratos/{criado!.Id}/encerrar", content: null);

        var response = await client.PostAsync($"/api/contratos/{criado.Id}/encerrar", content: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Depois_de_encerrar_um_contrato_o_ativo_pode_ser_alugado_de_novo()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatario1 = await CriarLocatarioAsync(client, "Locatário 1");
        var locatario2 = await CriarLocatarioAsync(client, "Locatário 2");
        var primeiro = await (await client.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatario1))).Content.ReadFromJsonAsync<ContratoDto>();
        await client.PostAsync($"/api/contratos/{primeiro!.Id}/encerrar", content: null);

        var response = await client.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatario2));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(StatusAtivo.Alugado, await StatusDoAtivoAsync(client, ativoId));
    }

    [Fact]
    public async Task Listar_retorna_apenas_contratos_do_usuario_autenticado()
    {
        var usuario1 = await factory.CriarClienteAutenticadoAsync();
        var ativoUsuario1 = await CriarAtivoAsync(usuario1);
        var locatarioUsuario1 = await CriarLocatarioAsync(usuario1);
        await usuario1.PostAsJsonAsync("/api/contratos", ContratoValido(ativoUsuario1, locatarioUsuario1));

        var usuario2 = await factory.CriarClienteAutenticadoAsync();
        var ativoUsuario2 = await CriarAtivoAsync(usuario2);
        var locatarioUsuario2 = await CriarLocatarioAsync(usuario2);
        await usuario2.PostAsJsonAsync("/api/contratos", ContratoValido(ativoUsuario2, locatarioUsuario2));

        var listagem = await usuario1.GetFromJsonAsync<List<ContratoDto>>("/api/contratos");

        var resultado = Assert.Single(listagem!);
        Assert.Equal(ativoUsuario1, resultado.AtivoId);
    }

    [Fact]
    public async Task Lancar_receita_vinculada_a_um_contrato_de_outro_usuario_retorna_404()
    {
        var dono = await factory.CriarClienteAutenticadoAsync();
        var ativoDono = await CriarAtivoAsync(dono);
        var locatarioDono = await CriarLocatarioAsync(dono);
        var contratoDono = await (await dono.PostAsJsonAsync("/api/contratos", ContratoValido(ativoDono, locatarioDono))).Content.ReadFromJsonAsync<ContratoDto>();

        var outroUsuario = await factory.CriarClienteAutenticadoAsync();
        var ativoOutroUsuario = await CriarAtivoAsync(outroUsuario);
        var lancamento = new LancamentoRequest(
            ativoOutroUsuario, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_500m,
            new DateOnly(2026, 3, 10), "Aluguel de março", contratoDono!.Id);

        var response = await outroUsuario.PostAsJsonAsync("/api/lancamentos", lancamento);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Lancar_receita_vinculada_a_contrato_de_outro_ativo_retorna_404()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativo1 = await CriarAtivoAsync(client, "Apê 1");
        var ativo2 = await CriarAtivoAsync(client, "Apê 2");
        var locatarioId = await CriarLocatarioAsync(client);
        var contratoDoAtivo1 = await (await client.PostAsJsonAsync("/api/contratos", ContratoValido(ativo1, locatarioId))).Content.ReadFromJsonAsync<ContratoDto>();

        var lancamento = new LancamentoRequest(
            ativo2, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_500m,
            new DateOnly(2026, 3, 10), "Aluguel de março", contratoDoAtivo1!.Id);

        var response = await client.PostAsJsonAsync("/api/lancamentos", lancamento);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Lancar_receita_vinculada_a_um_contrato_valido_retorna_201()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        var contrato = await (await client.PostAsJsonAsync("/api/contratos", ContratoValido(ativoId, locatarioId))).Content.ReadFromJsonAsync<ContratoDto>();

        var lancamento = new LancamentoRequest(
            ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_500m,
            new DateOnly(2026, 3, 10), "Aluguel de março", contrato!.Id);

        var response = await client.PostAsJsonAsync("/api/lancamentos", lancamento);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        Assert.Equal(contrato.Id, dto!.ContratoId);
    }

    [Fact]
    public async Task Chamar_endpoints_de_contratos_sem_token_retorna_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/contratos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
