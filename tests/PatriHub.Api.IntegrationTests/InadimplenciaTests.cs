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
    private static Task<Guid> CriarAtivoAsync(HttpClient client) => CenarioTestHelper.CriarAtivoAsync(client);

    private static Task<Guid> CriarLocatarioAsync(HttpClient client) => CenarioTestHelper.CriarLocatarioAsync(client);

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
    public async Task Contrato_com_vencimento_no_fim_do_mes_continua_avaliado_apos_virar_o_mes()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        // Vencimento em 28/01 — a carência de 5 dias (até 02/02) atravessa a virada do mês.
        var contrato = await CriarContratoAsync(client, ativoId, locatarioId, diaVencimento: 28, dataInicio: new DateOnly(2026, 1, 1));

        // Já é fevereiro, mas o vencimento relevante ainda é o de janeiro (28/01) — a checagem não
        // pode "esquecer" a competência de janeiro só porque o mês virou.
        await VerificarAsync(hoje: new DateOnly(2026, 2, 3));

        Assert.Equal(StatusContrato.Inadimplente, await StatusDoContratoAsync(client, contrato.Id));
    }

    [Fact]
    public async Task Contrato_com_vencimento_no_fim_do_mes_nao_vira_Inadimplente_logo_apos_virar_o_mes()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client);
        var locatarioId = await CriarLocatarioAsync(client);
        var contrato = await CriarContratoAsync(client, ativoId, locatarioId, diaVencimento: 28, dataInicio: new DateOnly(2026, 1, 1));

        // 01/02 ainda está dentro da carência do vencimento de 28/01 (carência até 02/02).
        await VerificarAsync(hoje: new DateOnly(2026, 2, 1));

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
