using System.Net;
using System.Net.Http.Json;
using PatriHub.Application.Ativos;
using PatriHub.Application.Lancamentos;
using PatriHub.Domain.Entidades;

namespace PatriHub.Api.IntegrationTests;

public sealed class LancamentosTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
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

    private static LancamentoRequest ReceitaValida(Guid ativoId, DateOnly? data = null) => new(
        ativoId,
        TipoLancamento.Receita,
        CategoriaLancamento.Aluguel,
        Valor: 1_500m,
        Data: data ?? new DateOnly(2026, 3, 10),
        Descricao: "Aluguel de março",
        ContratoId: null);

    private static LancamentoRequest DespesaValida(Guid ativoId, DateOnly? data = null) => new(
        ativoId,
        TipoLancamento.Despesa,
        CategoriaLancamento.Condominio,
        Valor: 400m,
        Data: data ?? new DateOnly(2026, 3, 12),
        Descricao: "Condomínio de março",
        ContratoId: null);

    private static async Task<Guid> CriarAtivoAsync(HttpClient client, string apelido = "Apê Centro")
    {
        var criado = await (await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido(apelido))).Content.ReadFromJsonAsync<AtivoDetalheDto>();
        return criado!.Id;
    }

    [Fact]
    public async Task Criar_receita_com_dados_validos_retorna_201_com_detalhe()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);

        var response = await client.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativoId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        Assert.NotNull(dto);
        Assert.Equal(ativoId, dto!.AtivoId);
        Assert.Equal(TipoLancamento.Receita, dto.Tipo);
        Assert.Equal(CategoriaLancamento.Aluguel, dto.Categoria);
        Assert.Equal(1_500m, dto.Valor);
    }

    [Fact]
    public async Task Criar_despesa_com_categoria_dentre_a_lista_fixa_retorna_201()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);

        var response = await client.PostAsJsonAsync("/api/lancamentos", DespesaValida(ativoId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        Assert.Equal(TipoLancamento.Despesa, dto!.Tipo);
        Assert.Equal(CategoriaLancamento.Condominio, dto.Categoria);
    }

    [Fact]
    public async Task Criar_despesa_com_categoria_de_receita_retorna_400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/lancamentos",
            DespesaValida(ativoId) with { Categoria = CategoriaLancamento.Aluguel });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_lancamento_com_valor_zero_retorna_400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);

        var response = await client.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativoId) with { Valor = 0m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_lancamento_para_ativo_de_outro_usuario_retorna_404()
    {
        var dono = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(dono);

        var outroUsuario = await factory.CriarClienteAutenticadoAsync();
        var response = await outroUsuario.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativoId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Criar_lancamento_para_ativo_inexistente_retorna_404()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var response = await client.PostAsJsonAsync("/api/lancamentos", ReceitaValida(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Atualizar_lancamento_troca_valor_e_categoria()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var criado = await (await client.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativoId))).Content.ReadFromJsonAsync<LancamentoDto>();

        var atualizarRequest = ReceitaValida(ativoId) with { Valor = 1_800m, Categoria = CategoriaLancamento.Outras };
        var response = await client.PutAsJsonAsync($"/api/lancamentos/{criado!.Id}", atualizarRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atualizado = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        Assert.Equal(1_800m, atualizado!.Valor);
        Assert.Equal(CategoriaLancamento.Outras, atualizado.Categoria);
    }

    [Fact]
    public async Task Atualizar_lancamento_com_AtivoId_diferente_retorna_400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoOriginal = await CriarAtivoAsync(client, "Apê 1");
        var outroAtivo = await CriarAtivoAsync(client, "Apê 2");
        var criado = await (await client.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativoOriginal))).Content.ReadFromJsonAsync<LancamentoDto>();

        var response = await client.PutAsJsonAsync($"/api/lancamentos/{criado!.Id}", ReceitaValida(outroAtivo));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var detalhe = await client.GetFromJsonAsync<LancamentoDto>($"/api/lancamentos/{criado.Id}");
        Assert.Equal(ativoOriginal, detalhe!.AtivoId);
    }

    [Fact]
    public async Task Atualizar_lancamento_de_outro_usuario_retorna_404()
    {
        var dono = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(dono);
        var criado = await (await dono.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativoId))).Content.ReadFromJsonAsync<LancamentoDto>();

        var outroUsuario = await factory.CriarClienteAutenticadoAsync();
        var response = await outroUsuario.PutAsJsonAsync($"/api/lancamentos/{criado!.Id}", ReceitaValida(ativoId) with { Valor = 1m });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Excluir_lancamento_faz_sumir_da_listagem_e_do_detalhe()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var criado = await (await client.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativoId))).Content.ReadFromJsonAsync<LancamentoDto>();

        var exclusao = await client.DeleteAsync($"/api/lancamentos/{criado!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, exclusao.StatusCode);

        var detalhe = await client.GetAsync($"/api/lancamentos/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, detalhe.StatusCode);

        var listagem = await client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos");
        Assert.DoesNotContain(listagem!, l => l.Id == criado.Id);
    }

    [Fact]
    public async Task Listar_filtra_por_ativo_periodo_e_tipo()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativo1 = await CriarAtivoAsync(client, "Apê 1");
        var ativo2 = await CriarAtivoAsync(client, "Apê 2");

        var receitaMarco = await (await client.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativo1, new DateOnly(2026, 3, 10)))).Content.ReadFromJsonAsync<LancamentoDto>();
        await client.PostAsJsonAsync("/api/lancamentos", DespesaValida(ativo1, new DateOnly(2026, 3, 12)));
        await client.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativo1, new DateOnly(2026, 4, 1)));
        await client.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativo2, new DateOnly(2026, 3, 10)));

        var listagem = await client.GetFromJsonAsync<List<LancamentoDto>>(
            $"/api/lancamentos?ativoId={ativo1}&dataInicio=2026-03-01&dataFim=2026-03-31&tipo=Receita");

        var resultado = Assert.Single(listagem!);
        Assert.Equal(receitaMarco!.Id, resultado.Id);
    }

    [Fact]
    public async Task Listar_retorna_apenas_lancamentos_do_usuario_autenticado()
    {
        var usuario1 = await factory.CriarClienteAutenticadoAsync();
        var usuario2 = await factory.CriarClienteAutenticadoAsync();

        var ativoUsuario1 = await CriarAtivoAsync(usuario1);
        var ativoUsuario2 = await CriarAtivoAsync(usuario2);
        await usuario1.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativoUsuario1));
        await usuario2.PostAsJsonAsync("/api/lancamentos", ReceitaValida(ativoUsuario2));

        var listagem = await usuario1.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos");

        var resultado = Assert.Single(listagem!);
        Assert.Equal(ativoUsuario1, resultado.AtivoId);
    }

    [Fact]
    public async Task Chamar_endpoints_de_lancamentos_sem_token_retorna_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/lancamentos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
