namespace PatriHub.Application.Dashboard;

/// <summary>
/// Métricas derivadas de um único Ativo — sempre recalculadas a partir de seus Lançamentos,
/// nunca persistidas (ver 01-SPEC-FUNCIONAL.md §5 e §6.4).
/// </summary>
/// <param name="RoiSobreValorFipeAtual">Terceira leitura de ROI, sobre ValorFipeAtual — null quando o Ativo não é Carro (ver issue #46).</param>
/// <param name="DivergenciaFipeAtual">Divergência entre ValorMercadoAtual e ValorFipeAtual — null quando o Ativo não é Carro.</param>
/// <param name="AlertaDivergenciaFipe">Se DivergenciaFipeAtual ultrapassa o limiar de alerta — null quando o Ativo não é Carro.</param>
public sealed record MetricasAtivoDto(
    Guid AtivoId,
    string Apelido,
    decimal LucroDoMes,
    decimal LucroAcumulado,
    decimal Yield,
    decimal RoiSobreValorAquisicao,
    decimal RoiSobreValorMercadoAtual,
    decimal Depreciacao,
    decimal? CustoDeOportunidade,
    decimal ProjecaoDeLucro,
    decimal? RoiSobreValorFipeAtual,
    decimal? DivergenciaFipeAtual,
    bool? AlertaDivergenciaFipe);

/// <summary>
/// Visão consolidada do patrimônio do usuário — soma de todos os seus Ativos, com a lista de
/// métricas por Ativo para comparação (ver AC "visão consolidada" e "comparar entre os Ativos").
/// </summary>
public sealed record PatrimonioConsolidadoDto(
    decimal LucroTotalDoMes,
    decimal LucroTotalAcumulado,
    IReadOnlyList<MetricasAtivoDto> Ativos);
