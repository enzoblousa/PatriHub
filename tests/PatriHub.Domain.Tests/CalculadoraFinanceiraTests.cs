using PatriHub.Domain.Calculos;
using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class CalculadoraFinanceiraTests
{
    private static Lancamento Receita(decimal valor, DateOnly data) =>
        Lancamento.Registrar(Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Receita, CategoriaLancamento.Aluguel, valor, data, null);

    private static Lancamento Despesa(decimal valor, DateOnly data) =>
        Lancamento.Registrar(Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Despesa, CategoriaLancamento.Iptu, valor, data, null);

    [Fact]
    public void LucroDoPeriodo_soma_receitas_e_subtrai_despesas_dentro_do_intervalo()
    {
        var lancamentos = new[]
        {
            Receita(1_500m, new DateOnly(2026, 3, 5)),
            Despesa(300m, new DateOnly(2026, 3, 10)),
            Despesa(150m, new DateOnly(2026, 3, 20)),
        };

        var lucro = CalculadoraFinanceira.LucroDoPeriodo(lancamentos, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        Assert.Equal(1_050m, lucro);
    }

    [Fact]
    public void LucroDoPeriodo_ignora_lancamentos_fora_do_intervalo()
    {
        var lancamentos = new[]
        {
            Receita(1_500m, new DateOnly(2026, 2, 28)),
            Receita(1_500m, new DateOnly(2026, 4, 1)),
        };

        var lucro = CalculadoraFinanceira.LucroDoPeriodo(lancamentos, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        Assert.Equal(0m, lucro);
    }

    [Fact]
    public void LucroDoPeriodo_sem_lancamentos_retorna_zero()
    {
        var lucro = CalculadoraFinanceira.LucroDoPeriodo([], new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        Assert.Equal(0m, lucro);
    }

    [Fact]
    public void LucroAcumulado_soma_todos_os_lancamentos_desde_a_aquisicao()
    {
        var dataAquisicao = new DateOnly(2024, 1, 1);
        var lancamentos = new[]
        {
            Receita(1_500m, new DateOnly(2024, 2, 1)),
            Receita(1_500m, new DateOnly(2026, 3, 1)),
            Despesa(500m, new DateOnly(2025, 6, 1)),
        };

        var lucroAcumulado = CalculadoraFinanceira.LucroAcumulado(lancamentos, dataAquisicao);

        Assert.Equal(2_500m, lucroAcumulado);
    }

    [Fact]
    public void LucroAcumulado_ignora_lancamentos_anteriores_a_aquisicao()
    {
        var dataAquisicao = new DateOnly(2026, 1, 1);
        var lancamentos = new[]
        {
            Receita(1_500m, new DateOnly(2025, 12, 31)),
        };

        var lucroAcumulado = CalculadoraFinanceira.LucroAcumulado(lancamentos, dataAquisicao);

        Assert.Equal(0m, lucroAcumulado);
    }

    [Theory]
    [InlineData(300_000, 350_000, -50_000)]
    [InlineData(300_000, 250_000, 50_000)]
    [InlineData(300_000, 300_000, 0)]
    public void Depreciacao_e_ValorAquisicao_menos_ValorMercadoAtual(decimal valorAquisicao, decimal valorMercadoAtual, decimal esperado)
    {
        var depreciacao = CalculadoraFinanceira.Depreciacao(valorAquisicao, valorMercadoAtual);

        Assert.Equal(esperado, depreciacao);
    }

    [Fact]
    public void ReceitaDeAluguelLiquidaDoPeriodo_soma_apenas_receita_de_aluguel_e_subtrai_todas_as_despesas()
    {
        var lancamentos = new[]
        {
            Receita(3_500m, new DateOnly(2026, 3, 5)), // categoria Aluguel
            Lancamento.Registrar(Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Receita, CategoriaLancamento.TaxaDeServico, 200m, new DateOnly(2026, 3, 6), null),
            Despesa(500m, new DateOnly(2026, 3, 10)),
        };

        var receitaLiquida = CalculadoraFinanceira.ReceitaDeAluguelLiquidaDoPeriodo(lancamentos, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        // 3_500 (aluguel) − 500 (despesa) — os 200 de TaxaDeServico não entram (não é aluguel).
        Assert.Equal(3_000m, receitaLiquida);
    }

    [Fact]
    public void ReceitaDeAluguelLiquidaDoPeriodo_ignora_lancamentos_fora_do_intervalo()
    {
        var lancamentos = new[]
        {
            Receita(3_500m, new DateOnly(2026, 2, 28)),
        };

        var receitaLiquida = CalculadoraFinanceira.ReceitaDeAluguelLiquidaDoPeriodo(lancamentos, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        Assert.Equal(0m, receitaLiquida);
    }

    [Fact]
    public void Yield_divide_a_receita_de_aluguel_liquida_pelo_ValorMercadoAtual()
    {
        var yield = CalculadoraFinanceira.Yield(receitaDeAluguelLiquida: 3_500m, valorMercadoAtual: 350_000m);

        Assert.Equal(0.01m, yield);
    }

    [Fact]
    public void Yield_com_ValorMercadoAtual_zero_retorna_zero_em_vez_de_lancar()
    {
        var yield = CalculadoraFinanceira.Yield(receitaDeAluguelLiquida: 1_000m, valorMercadoAtual: 0m);

        Assert.Equal(0m, yield);
    }

    [Fact]
    public void LucroTotalComValorizacao_soma_lucro_acumulado_e_valorizacao_do_ativo()
    {
        // Ativo valorizou 50.000 (350.000 − 300.000) além do lucro acumulado de fluxo de caixa.
        var lucroTotal = CalculadoraFinanceira.LucroTotalComValorizacao(
            lucroAcumulado: 10_000m, valorAquisicao: 300_000m, valorMercadoAtual: 350_000m);

        Assert.Equal(60_000m, lucroTotal);
    }

    [Fact]
    public void LucroTotalComValorizacao_subtrai_depreciacao_quando_ativo_perdeu_valor()
    {
        var lucroTotal = CalculadoraFinanceira.LucroTotalComValorizacao(
            lucroAcumulado: 10_000m, valorAquisicao: 300_000m, valorMercadoAtual: 250_000m);

        Assert.Equal(-40_000m, lucroTotal);
    }

    [Fact]
    public void Roi_divide_o_lucro_total_pela_base_informada()
    {
        var roiSobreAquisicao = CalculadoraFinanceira.Roi(lucroTotalComValorizacao: 60_000m, valorBase: 300_000m);
        var roiSobreMercado = CalculadoraFinanceira.Roi(lucroTotalComValorizacao: 60_000m, valorBase: 350_000m);

        Assert.Equal(0.2m, roiSobreAquisicao);
        Assert.Equal(60_000m / 350_000m, roiSobreMercado);
    }

    [Fact]
    public void Roi_com_base_zero_retorna_zero_em_vez_de_lancar()
    {
        var roi = CalculadoraFinanceira.Roi(lucroTotalComValorizacao: 1_000m, valorBase: 0m);

        Assert.Equal(0m, roi);
    }

    [Fact]
    public void CustoDeOportunidade_multiplica_ValorMercadoAtual_pela_taxa_anual()
    {
        var custo = CalculadoraFinanceira.CustoDeOportunidade(valorMercadoAtual: 350_000m, taxaReferenciaAnual: 0.12m);

        Assert.Equal(42_000m, custo);
    }

    [Fact]
    public void ProjecaoDeLucro_e_a_media_do_lucro_dos_3_meses_completos_anteriores()
    {
        var lancamentos = new[]
        {
            Receita(1_000m, new DateOnly(2026, 3, 5)), // dezembro/2025
            Receita(2_000m, new DateOnly(2026, 4, 5)), // janeiro/2026
            Receita(3_000m, new DateOnly(2026, 5, 5)), // fevereiro/2026
        };

        var projecao = CalculadoraFinanceira.ProjecaoDeLucro(lancamentos, mesReferencia: new DateOnly(2026, 6, 1));

        Assert.Equal(2_000m, projecao);
    }

    [Fact]
    public void ProjecaoDeLucro_ignora_lancamentos_do_mes_de_referencia()
    {
        var lancamentos = new[]
        {
            Receita(3_000m, new DateOnly(2026, 5, 5)), // fevereiro, entra na média
            Receita(999_999m, new DateOnly(2026, 6, 10)), // mês de referência, não entra
        };

        var projecao = CalculadoraFinanceira.ProjecaoDeLucro(lancamentos, mesReferencia: new DateOnly(2026, 6, 15));

        Assert.Equal(1_000m, projecao);
    }

    [Fact]
    public void ProjecaoDeLucro_sem_lancamentos_nos_ultimos_3_meses_retorna_zero()
    {
        var projecao = CalculadoraFinanceira.ProjecaoDeLucro([], mesReferencia: new DateOnly(2026, 6, 15));

        Assert.Equal(0m, projecao);
    }

    [Theory]
    [InlineData(2026, 2, "2026-02-01", "2026-02-28")]
    [InlineData(2026, 3, "2026-03-01", "2026-03-31")]
    [InlineData(2024, 2, "2024-02-01", "2024-02-29")] // ano bissexto
    public void IntervaloDoMes_retorna_o_primeiro_e_o_ultimo_dia_do_mes(int ano, int mes, string inicioEsperado, string fimEsperado)
    {
        var (inicio, fim) = CalculadoraFinanceira.IntervaloDoMes(new DateOnly(ano, mes, 15));

        Assert.Equal(DateOnly.Parse(inicioEsperado), inicio);
        Assert.Equal(DateOnly.Parse(fimEsperado), fim);
    }
}
