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

    private static decimal ValorComSinal(Lancamento lancamento) =>
        lancamento.Tipo == TipoLancamento.Receita ? lancamento.Valor : -lancamento.Valor;
}
