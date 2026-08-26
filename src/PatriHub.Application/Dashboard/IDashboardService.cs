namespace PatriHub.Application.Dashboard;

/// <summary>
/// Métricas financeiras derivadas de cada Ativo do usuário e a visão consolidada do patrimônio.
/// Toda operação recebe o `usuarioId` extraído do JWT e filtra implicitamente por ele — nunca por
/// um parâmetro vindo do cliente (ver 01-SPEC-FUNCIONAL.md §7). Todo valor retornado é calculado
/// on-the-fly a partir de Ativos/Lançamentos, nunca persistido (ver 01-SPEC-FUNCIONAL.md §6.4).
/// </summary>
public interface IDashboardService
{
    /// <param name="usuarioId">Dono dos Ativos — nunca vindo do cliente.</param>
    /// <param name="taxaReferenciaAnual">
    /// Taxa de referência anual (ex.: CDI/Selic), informada manualmente pelo usuário para o
    /// cálculo do custo de oportunidade; null omite <see cref="MetricasAtivoDto.CustoDeOportunidade"/>
    /// (ver 01-SPEC-FUNCIONAL.md §5).
    /// </param>
    /// <param name="hoje">Data de referência para "mês corrente" e projeção; usada em teste para não depender do relógio real.</param>
    Task<PatrimonioConsolidadoDto> ObterDashboardAsync(Guid usuarioId, decimal? taxaReferenciaAnual = null, DateOnly? hoje = null);
}
