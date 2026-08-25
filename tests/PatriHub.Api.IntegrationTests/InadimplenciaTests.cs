using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PatriHub.Application.Ativos;
using PatriHub.Application.Contratos;
using PatriHub.Application.Lancamentos;
using PatriHub.Application.Locatarios;
using PatriHub.Domain.Entidades;

namespace PatriHub.Api.IntegrationTests;

/// <summary>
/// Chama <see cref="IVerificacaoInadimplenciaService.VerificarAsync"/> diretamente (resolvido do
/// container de DI), sem depender do timer do BackgroundService — ver ADR-0003 e critério de
/// aceite do ticket #6.
/// </summary>
public sealed class InadimplenciaTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
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

    private static async Task<Guid> CriarAtivoAsync(HttpClient client) =>
        (await (await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido())).Content.ReadFromJsonAsync<AtivoDetalheDto>())!.Id;

    private static async Task<Guid> CriarLocatarioAsync(HttpClient client) =>
        (await (await client.PostAsJsonAsync("/api/locatarios", LocatarioValido())).Content.ReadFromJsonAsync<LocatarioDto>())!.Id;

    private static async Task<ContratoDto> CriarContratoAsync(
        HttpClient client, Guid ativoId, Guid locatarioId, int diaVencimento, DateOnly dataInicio)
    {
        var request = new ContratoRequest(ativoId, locatarioId, ValorAluguelMensal: 1_500m, diaVencimento, dataInicio, DataFim: null);
        var criado = await client.PostAsJsonAsync("/api/contratos", request);
        return (await criado.Content.ReadFromJsonAsync<ContratoDto>())!;
    }

    private async Task VerificarAsync(DateOnly hoje)
    {
        using var scope = factory.Services.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IVerificacaoInadimplenciaService>();
        await servico.VerificarAsync(hoje);
    }

    private static async Task<StatusContrato> StatusDoContratoAsync(HttpClient client, Guid contratoId)
    {
        var listagem = await client.GetFromJsonAsync<List<ContratoDto>>("/api/contratos");
        return listagem!.Single(c => c.Id == contratoId).Status;
    }

    [Fact]
    public async Task Contrato_sem_lancamento_apos_5_dias_de_carencia_vira_Inadimplente()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        var contrato = await CriarContratoAsync(client, ativoId, locatarioId, diaVencimento: 10, dataInicio: new DateOnly(2026, 1, 1));

        // Vencimento em 10/03, carência de 5 dias termina em 15/03 — 16/03 já está fora da carência.
        await VerificarAsync(hoje: new DateOnly(2026, 3, 16));

        Assert.Equal(StatusContrato.Inadimplente, await StatusDoContratoAsync(client, contrato.Id));
    }

    [Fact]
    public async Task Contrato_ainda_dentro_da_carencia_nao_vira_Inadimplente()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        var contrato = await CriarContratoAsync(client, ativoId, locatarioId, diaVencimento: 10, dataInicio: new DateOnly(2026, 1, 1));

        // Vencimento em 10/03, carência de 5 dias — 15/03 ainda está dentro da carência.
        await VerificarAsync(hoje: new DateOnly(2026, 3, 15));

        Assert.Equal(StatusContrato.Ativo, await StatusDoContratoAsync(client, contrato.Id));
    }

    [Fact]
    public async Task Contrato_com_lancamento_correspondente_no_mes_nao_vira_Inadimplente()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        var contrato = await CriarContratoAsync(client, ativoId, locatarioId, diaVencimento: 10, dataInicio: new DateOnly(2026, 1, 1));
        var lancamento = new LancamentoRequest(
            ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_500m,
            new DateOnly(2026, 3, 8), "Aluguel de março", contrato.Id);
        await client.PostAsJsonAsync("/api/lancamentos", lancamento);

        await VerificarAsync(hoje: new DateOnly(2026, 3, 16));

        Assert.Equal(StatusContrato.Ativo, await StatusDoContratoAsync(client, contrato.Id));
    }

    [Fact]
    public async Task Contrato_com_lancamento_de_outro_mes_de_competencia_vira_Inadimplente()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        var contrato = await CriarContratoAsync(client, ativoId, locatarioId, diaVencimento: 10, dataInicio: new DateOnly(2026, 1, 1));

        // Lançamento existe, mas é de fevereiro — não cobre a competência de março.
        var lancamento = new LancamentoRequest(
            ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_500m,
            new DateOnly(2026, 2, 8), "Aluguel de fevereiro", contrato.Id);
        await client.PostAsJsonAsync("/api/lancamentos", lancamento);

        await VerificarAsync(hoje: new DateOnly(2026, 3, 16));

        Assert.Equal(StatusContrato.Inadimplente, await StatusDoContratoAsync(client, contrato.Id));
    }

    [Fact]
    public async Task Contrato_cujo_vencimento_do_mes_e_anterior_ao_inicio_do_contrato_nao_vira_Inadimplente()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        // Contrato começa depois do dia de vencimento deste mês (dia 20 > dia 10) — o vencimento
        // de março ainda não se aplica a este Contrato.
        var contrato = await CriarContratoAsync(client, ativoId, locatarioId, diaVencimento: 10, dataInicio: new DateOnly(2026, 3, 20));

        await VerificarAsync(hoje: new DateOnly(2026, 3, 30));

        Assert.Equal(StatusContrato.Ativo, await StatusDoContratoAsync(client, contrato.Id));
    }

    [Fact]
    public async Task Contrato_ja_Encerrado_nao_vira_Inadimplente()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        var contrato = await CriarContratoAsync(client, ativoId, locatarioId, diaVencimento: 10, dataInicio: new DateOnly(2026, 1, 1));
        await client.PostAsync($"/api/contratos/{contrato.Id}/encerrar", content: null);

        await VerificarAsync(hoje: new DateOnly(2026, 3, 16));

        Assert.Equal(StatusContrato.Encerrado, await StatusDoContratoAsync(client, contrato.Id));
    }
}
