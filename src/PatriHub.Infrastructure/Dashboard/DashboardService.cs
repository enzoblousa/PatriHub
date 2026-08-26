using Microsoft.EntityFrameworkCore;
using PatriHub.Application.Dashboard;
using PatriHub.Domain.Calculos;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Dashboard;

/// <summary>
/// Consulta filtra por `UsuarioId` diretamente na query (nunca checa dono depois de carregar) —
/// mesmo padrão de isolamento por conta do <see cref="PatriHub.Infrastructure.Ativos.AtivoService"/>.
/// Toda métrica é calculada em memória por <see cref="CalculadoraFinanceira"/>, a partir dos
/// Lançamentos carregados aqui — nada é persistido (ver 01-SPEC-FUNCIONAL.md §6.4).
/// </summary>
public sealed class DashboardService(PatriHubDbContext db) : IDashboardService
{
    public async Task<PatrimonioConsolidadoDto> ObterDashboardAsync(Guid usuarioId, decimal? taxaReferenciaAnual = null, DateOnly? hoje = null)
    {
        var dataReferencia = hoje ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var (inicioMes, fimMes) = CalculadoraFinanceira.IntervaloDoMes(dataReferencia);

        var ativos = await db.Ativos
            .Where(a => a.UsuarioId == usuarioId && a.ExcluidoEm == null)
            .OrderByDescending(a => a.CriadoEm)
            .ToListAsync();

        if (ativos.Count == 0)
        {
            return new PatrimonioConsolidadoDto(0m, 0m, []);
        }

        // Um único SELECT cobrindo todos os Ativos do usuário, desde a menor DataAquisicao entre
        // eles até o fim do mês corrente — evita 1 query de Lançamentos por Ativo (mesmo padrão do
        // AtivoService.ListarAsync). É uma janela superconjunto: sempre <= à DataAquisicao de
        // qualquer Ativo individual, então nenhum Lançamento relevante fica de fora.
        var dataMaisAntiga = ativos.Min(a => a.DataAquisicao);
        var lancamentosPorAtivo = (await db.Lancamentos
                .Where(l => l.UsuarioId == usuarioId && l.Data >= dataMaisAntiga && l.Data <= fimMes)
                .ToListAsync())
            .ToLookup(l => l.AtivoId);

        var metricas = ativos
            .Select(a => CalcularMetricas(a, lancamentosPorAtivo[a.Id], inicioMes, fimMes, dataReferencia, taxaReferenciaAnual))
            .ToList();

        return new PatrimonioConsolidadoDto(
            LucroTotalDoMes: metricas.Sum(m => m.LucroDoMes),
            LucroTotalAcumulado: metricas.Sum(m => m.LucroAcumulado),
            Ativos: metricas);
    }

    private static MetricasAtivoDto CalcularMetricas(
        Ativo ativo,
        IEnumerable<Lancamento> lancamentosDoAtivo,
        DateOnly inicioMes,
        DateOnly fimMes,
        DateOnly dataReferencia,
        decimal? taxaReferenciaAnual)
    {
        var lancamentos = lancamentosDoAtivo as IReadOnlyCollection<Lancamento> ?? lancamentosDoAtivo.ToList();

        var lucroDoMes = CalculadoraFinanceira.LucroDoPeriodo(lancamentos, inicioMes, fimMes);
        var lucroAcumulado = CalculadoraFinanceira.LucroAcumulado(lancamentos, ativo.DataAquisicao);
        var lucroTotalComValorizacao = CalculadoraFinanceira.LucroTotalComValorizacao(
            lucroAcumulado, ativo.ValorAquisicao, ativo.ValorMercadoAtual);

        return new MetricasAtivoDto(
            ativo.Id,
            ativo.Apelido,
            LucroDoMes: lucroDoMes,
            LucroAcumulado: lucroAcumulado,
            Yield: CalculadoraFinanceira.Yield(
                CalculadoraFinanceira.ReceitaDeAluguelLiquidaDoPeriodo(lancamentos, inicioMes, fimMes),
                ativo.ValorMercadoAtual),
            RoiSobreValorAquisicao: CalculadoraFinanceira.Roi(lucroTotalComValorizacao, ativo.ValorAquisicao),
            RoiSobreValorMercadoAtual: CalculadoraFinanceira.Roi(lucroTotalComValorizacao, ativo.ValorMercadoAtual),
            Depreciacao: CalculadoraFinanceira.Depreciacao(ativo.ValorAquisicao, ativo.ValorMercadoAtual),
            CustoDeOportunidade: taxaReferenciaAnual is { } taxa
                ? CalculadoraFinanceira.CustoDeOportunidade(ativo.ValorMercadoAtual, taxa)
                : null,
            ProjecaoDeLucro: CalculadoraFinanceira.ProjecaoDeLucro(lancamentos, dataReferencia));
    }
}
