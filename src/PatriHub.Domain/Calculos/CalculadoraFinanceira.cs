using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Calculos;

/// <summary>
/// Cálculos financeiros derivados de um Ativo e seus Lançamentos — funções puras, sem
/// dependência de banco (seam 1 do ticket #4), testadas isoladamente em
/// <c>PatriHub.Domain.Tests</c>. Ver 02-PLANO-TECNICO.md §5.
/// </summary>
public static class CalculadoraFinanceira
{
    /// <summary>Lucro do período = Σ Receitas(período) − Σ Despesas(período).</summary>
    public static decimal LucroDoPeriodo(IEnumerable<Lancamento> lancamentos, DateOnly inicio, DateOnly fim) =>
        lancamentos
            .Where(l => l.Data >= inicio && l.Data <= fim)
            .Sum(ValorComSinal);

    /// <summary>Lucro acumulado = soma de todos os lançamentos desde a aquisição do Ativo.</summary>
    public static decimal LucroAcumulado(IEnumerable<Lancamento> lancamentos, DateOnly dataAquisicao) =>
        lancamentos
            .Where(l => l.Data >= dataAquisicao)
            .Sum(ValorComSinal);

    /// <summary>Depreciação = ValorAquisição − ValorMercadoAtual (ambos informados manualmente).</summary>
    public static decimal Depreciacao(decimal valorAquisicao, decimal valorMercadoAtual) =>
        valorAquisicao - valorMercadoAtual;

    /// <summary>
    /// Receita de aluguel líquida do período = Σ Receitas(categoria Aluguel) − Σ Despesas, no
    /// intervalo — a base de renda do <see cref="Yield"/>. Distinta de <see cref="LucroDoPeriodo"/>,
    /// que soma toda categoria de Receita (TaxaDeServiço, MultaPorAtraso, Outras também); aqui só a
    /// renda de aluguel entra no numerador, mas toda Despesa do período ainda é descontada (ver
    /// 01-SPEC-FUNCIONAL.md §5 e CONTEXT.md — "Yield... receita de aluguel líquida").
    /// </summary>
    public static decimal ReceitaDeAluguelLiquidaDoPeriodo(IEnumerable<Lancamento> lancamentos, DateOnly inicio, DateOnly fim) =>
        lancamentos
            .Where(l => l.Data >= inicio && l.Data <= fim)
            .Where(l => l.Tipo == TipoLancamento.Despesa || l.Categoria == CategoriaLancamento.Aluguel)
            .Sum(ValorComSinal);

    /// <summary>
    /// Yield = Receita de aluguel líquida do período (<see cref="ReceitaDeAluguelLiquidaDoPeriodo"/>)
    /// ÷ ValorMercadoAtual — retorno só da renda de aluguel, sem valorização/depreciação. Distinto de
    /// <see cref="Roi"/>, que inclui a variação do valor de mercado e toda categoria de Receita (ver
    /// CONTEXT.md — "Avoid: usar ROI para essa métrica").
    /// </summary>
    public static decimal Yield(decimal receitaDeAluguelLiquida, decimal valorMercadoAtual) =>
        valorMercadoAtual == 0 ? 0m : receitaDeAluguelLiquida / valorMercadoAtual;

    /// <summary>
    /// Lucro total = Lucro acumulado (fluxo de caixa) + valorização do Ativo (o inverso de
    /// <see cref="Depreciacao"/>) — a base do numerador do ROI, que ao contrário do Yield inclui a
    /// variação do valor de mercado desde a aquisição.
    /// </summary>
    public static decimal LucroTotalComValorizacao(decimal lucroAcumulado, decimal valorAquisicao, decimal valorMercadoAtual) =>
        lucroAcumulado + (valorMercadoAtual - valorAquisicao);

    /// <summary>
    /// ROI = Lucro total (<see cref="LucroTotalComValorizacao"/>) ÷ uma base de investimento —
    /// chamado duas vezes pelo chamador, uma com `ValorAquisição` e outra com `ValorMercadoAtual`
    /// como <paramref name="valorBase"/>, exibidas lado a lado (ver 01-SPEC-FUNCIONAL.md §5).
    /// </summary>
    public static decimal Roi(decimal lucroTotalComValorizacao, decimal valorBase) =>
        valorBase == 0 ? 0m : lucroTotalComValorizacao / valorBase;

    /// <summary>
    /// Custo de oportunidade = ValorMercadoAtual × taxa de referência anual informada manualmente
    /// pelo usuário (ex.: CDI/Selic) — sem integração com índice oficial no MVP.
    /// </summary>
    public static decimal CustoDeOportunidade(decimal valorMercadoAtual, decimal taxaReferenciaAnual) =>
        valorMercadoAtual * taxaReferenciaAnual;

    /// <summary>
    /// Projeção de lucro do próximo mês = média do Lucro dos 3 meses completos anteriores a
    /// <paramref name="mesReferencia"/> (projeção linear simples — sem tendência, ver
    /// 01-SPEC-FUNCIONAL.md §6.4). O mês de <paramref name="mesReferencia"/> em si não entra na média.
    /// </summary>
    public static decimal ProjecaoDeLucro(IEnumerable<Lancamento> lancamentos, DateOnly mesReferencia)
    {
        var lista = lancamentos as IReadOnlyCollection<Lancamento> ?? lancamentos.ToList();
        var inicioDoMesReferencia = new DateOnly(mesReferencia.Year, mesReferencia.Month, 1);

        var somaDosTresMeses = 0m;
        for (var mesesAtras = 1; mesesAtras <= 3; mesesAtras++)
        {
            var inicioDoMes = inicioDoMesReferencia.AddMonths(-mesesAtras);
            var fimDoMes = inicioDoMes.AddMonths(1).AddDays(-1);
            somaDosTresMeses += LucroDoPeriodo(lista, inicioDoMes, fimDoMes);
        }

        return somaDosTresMeses / 3m;
    }

    /// <summary>Primeiro e último dia do mês de <paramref name="data"/> — usado por qualquer cálculo de "mês corrente" (ver AtivoService.ListarAsync e DashboardService).</summary>
    public static (DateOnly Inicio, DateOnly Fim) IntervaloDoMes(DateOnly data)
    {
        var inicio = new DateOnly(data.Year, data.Month, 1);
        return (inicio, inicio.AddMonths(1).AddDays(-1));
    }

    private static decimal ValorComSinal(Lancamento lancamento) =>
        lancamento.Tipo == TipoLancamento.Receita ? lancamento.Valor : -lancamento.Valor;
}
