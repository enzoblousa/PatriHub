using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PatriHub.Application.Dashboard;
using PatriHub.Application.Lancamentos;
using PatriHub.Domain.Entidades;

namespace PatriHub.Api.IntegrationTests;

public sealed class DashboardTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
{
    private static Task<Guid> CriarAtivoAsync(HttpClient client) => CenarioTestHelper.CriarAtivoAsync(client);

    private static Task CriarLancamentoAsync(HttpClient client, Guid ativoId, TipoLancamento tipo, CategoriaLancamento categoria, decimal valor, DateOnly data) =>
        client.PostAsJsonAsync("/api/lancamentos", new LancamentoRequest(ativoId, tipo, categoria, valor, data, null, null));

    /// <summary>Resolve <see cref="IDashboardService"/> diretamente do container de DI, sem passar pela camada HTTP — necessário para controlar `hoje` nos testes de projeção (mesmo padrão de InadimplenciaTests, ver ADR-0003).</summary>
    private async Task<PatrimonioConsolidadoDto> ObterDashboardDiretoAsync(Guid usuarioId, decimal? taxaReferenciaAnual = null, DateOnly? hoje = null)
    {
        using var scope = factory.Services.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IDashboardService>();
        return await servico.ObterDashboardAsync(usuarioId, taxaReferenciaAnual, hoje);
    }

    [Fact]
    public async Task Obter_sem_token_retorna_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Obter_sem_ativos_retorna_totais_zero_e_lista_vazia()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var dashboard = await client.GetFromJsonAsync<PatrimonioConsolidadoDto>("/api/dashboard");

        Assert.Equal(0m, dashboard!.LucroTotalDoMes);
        Assert.Equal(0m, dashboard.LucroTotalAcumulado);
        Assert.Empty(dashboard.Ativos);
    }

    [Fact]
    public async Task Obter_retorna_metricas_do_ativo_e_totais_consolidados()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var ativoId = await CriarAtivoAsync(client); // ValorAquisicao 300_000, ValorMercadoAtual 350_000
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        await CriarLancamentoAsync(client, ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 3_500m, hoje);
        // Receita fora da categoria Aluguel: entra no Lucro do mês, mas não na "receita de aluguel
        // líquida" do Yield — ver ReceitaDeAluguelLiquidaDoPeriodo.
        await CriarLancamentoAsync(client, ativoId, TipoLancamento.Receita, CategoriaLancamento.TaxaDeServico, 800m, hoje);
        await CriarLancamentoAsync(client, ativoId, TipoLancamento.Despesa, CategoriaLancamento.Condominio, 500m, hoje);

        var dashboard = await client.GetFromJsonAsync<PatrimonioConsolidadoDto>("/api/dashboard");

        var metricas = Assert.Single(dashboard!.Ativos);
        Assert.Equal(ativoId, metricas.AtivoId);
        Assert.Equal(3_800m, metricas.LucroDoMes); // 3_500 + 800 − 500
        Assert.Equal(3_800m, metricas.LucroAcumulado);
        // Yield usa só a receita de Aluguel líquida de despesas (3_500 − 500), não o Lucro do mês inteiro.
        Assert.Equal(3_000m / 350_000m, metricas.Yield);
        Assert.NotEqual(metricas.LucroDoMes / 350_000m, metricas.Yield);
        Assert.Equal(-50_000m, metricas.Depreciacao);
        // Lucro total com valorização = 3_800 (lucro acumulado) + 50_000 (valorização) = 53_800.
        Assert.Equal(53_800m / 300_000m, metricas.RoiSobreValorAquisicao);
        Assert.Equal(53_800m / 350_000m, metricas.RoiSobreValorMercadoAtual);
        Assert.Null(metricas.CustoDeOportunidade);
        // Imóvel não tem ValorFipeAtual — leituras específicas de Carro ficam null (ver issue #46).
        Assert.Null(metricas.RoiSobreValorFipeAtual);
        Assert.Null(metricas.DivergenciaFipeAtual);
        Assert.Null(metricas.AlertaDivergenciaFipe);

        Assert.Equal(3_800m, dashboard.LucroTotalDoMes);
        Assert.Equal(3_800m, dashboard.LucroTotalAcumulado);
    }

    [Fact]
    public async Task Obter_retorna_ROI_sobre_FIPE_para_Carro()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        await CenarioTestHelper.CriarCarroAsync(client); // ValorAquisicao 90_000, ValorMercadoAtual 85_000, ValorFipeAtual 80_000

        var dashboard = await client.GetFromJsonAsync<PatrimonioConsolidadoDto>("/api/dashboard");

        var metricas = Assert.Single(dashboard!.Ativos);
        // Sem Lançamentos: lucro acumulado 0, lucro total com valorização = 85_000 − 90_000 = −5_000.
        Assert.Equal(-5_000m / 80_000m, metricas.RoiSobreValorFipeAtual);
        Assert.Equal((85_000m - 80_000m) / 80_000m, metricas.DivergenciaFipeAtual);
        Assert.False(metricas.AlertaDivergenciaFipe); // 6,25% de divergência, abaixo do limiar de 15%
    }

    [Fact]
    public async Task Obter_sinaliza_alerta_quando_divergencia_da_FIPE_ultrapassa_o_limiar()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        // ValorMercadoAtual 85_000 vs. ValorFipeAtual 60_000 — 41,7% de divergência, acima do limiar de 15%.
        await CenarioTestHelper.CriarCarroAsync(client, valorFipeAtual: 60_000m);

        var dashboard = await client.GetFromJsonAsync<PatrimonioConsolidadoDto>("/api/dashboard");

        var metricas = Assert.Single(dashboard!.Ativos);
        Assert.True(metricas.AlertaDivergenciaFipe);
    }

    [Fact]
    public async Task Obter_com_taxaReferenciaAnual_retorna_custo_de_oportunidade_calculado()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        await CriarAtivoAsync(client); // ValorMercadoAtual 350_000

        var dashboard = await client.GetFromJsonAsync<PatrimonioConsolidadoDto>("/api/dashboard?taxaReferenciaAnual=0.12");

        var metricas = Assert.Single(dashboard!.Ativos);
        Assert.Equal(42_000m, metricas.CustoDeOportunidade);
    }

    [Fact]
    public async Task Obter_retorna_apenas_ativos_do_usuario_autenticado_para_comparacao()
    {
        var usuario1 = await factory.CriarClienteAutenticadoAsync();
        var usuario2 = await factory.CriarClienteAutenticadoAsync();
        var ativoUsuario1 = await CriarAtivoAsync(usuario1);
        await CriarAtivoAsync(usuario2);
        await CriarAtivoAsync(usuario1); // dois ativos do mesmo usuário, para a comparação lado a lado

        var dashboard = await usuario1.GetFromJsonAsync<PatrimonioConsolidadoDto>("/api/dashboard");

        Assert.Equal(2, dashboard!.Ativos.Count);
        Assert.Contains(dashboard.Ativos, m => m.AtivoId == ativoUsuario1);
    }

    [Fact]
    public async Task Obter_projeta_lucro_como_media_do_lucro_dos_3_meses_completos_anteriores()
    {
        var (client, usuarioId) = await factory.CriarClienteAutenticadoComIdAsync();
        var ativoId = await CriarAtivoAsync(client);
        await CriarLancamentoAsync(client, ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_000m, new DateOnly(2026, 3, 5));
        await CriarLancamentoAsync(client, ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 2_000m, new DateOnly(2026, 4, 5));
        await CriarLancamentoAsync(client, ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 3_000m, new DateOnly(2026, 5, 5));
        // Mês de referência (junho) não deve entrar na média.
        await CriarLancamentoAsync(client, ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 999_999m, new DateOnly(2026, 6, 10));

        var dashboard = await ObterDashboardDiretoAsync(usuarioId, hoje: new DateOnly(2026, 6, 15));

        var metricas = Assert.Single(dashboard.Ativos);
        Assert.Equal(2_000m, metricas.ProjecaoDeLucro);
    }
}
