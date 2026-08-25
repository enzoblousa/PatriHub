using Microsoft.EntityFrameworkCore;
using PatriHub.Application.Contratos;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Contratos;

/// <summary>
/// Varre todos os Contratos `Ativo` (de qualquer usuário — é um job de sistema, ver
/// <see cref="IVerificacaoInadimplenciaService"/>) e marca `Inadimplente` os que passaram da
/// carência de <see cref="DiasDeCarencia"/> sem Lançamento correspondente no mês de competência.
/// </summary>
public sealed class VerificacaoInadimplenciaService(PatriHubDbContext db) : IVerificacaoInadimplenciaService
{
    private const int DiasDeCarencia = 5;

    public async Task VerificarAsync(DateOnly? hoje = null)
    {
        var dataReferencia = hoje ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

        var contratosAtivos = await db.Contratos
            .Where(c => c.Status == StatusContrato.Ativo)
            .ToListAsync();

        // O vencimento relevante de cada Contrato pode cair no mês de `dataReferencia` (se já
        // ocorreu) ou no mês anterior (se o vencimento deste mês ainda não chegou) — ver
        // VencimentoRelevante. Cada um carrega seu próprio mês de competência, então dois
        // Contratos nesta lista podem estar sendo avaliados contra meses diferentes.
        var candidatos = contratosAtivos
            .Select(c => (Contrato: c, Vencimento: VencimentoRelevante(c, dataReferencia)))
            .Where(x => x.Vencimento is { } vencimento && dataReferencia > vencimento.AddDays(DiasDeCarencia))
            .Select(x => (x.Contrato, Vencimento: x.Vencimento!.Value))
            .ToList();

        if (candidatos.Count == 0)
        {
            return;
        }

        // Intervalo cobrindo os dois meses de competência possíveis entre os candidatos (mês
        // anterior e mês atual relativos a `dataReferencia`) — um único SELECT, cada Contrato é
        // comparado contra o seu próprio mês de competência (`Vencimento`) abaixo.
        var inicioIntervalo = new DateOnly(dataReferencia.Year, dataReferencia.Month, 1).AddMonths(-1);
        var fimIntervalo = new DateOnly(dataReferencia.Year, dataReferencia.Month, 1).AddMonths(1).AddDays(-1);
        var idsCandidatos = candidatos.Select(x => x.Contrato.Id).ToList();

        var lancamentosPorContrato = (await db.Lancamentos
                .Where(l => l.ContratoId != null
                    && idsCandidatos.Contains(l.ContratoId.Value)
                    && l.Tipo == TipoLancamento.Receita
                    && l.Categoria == CategoriaLancamento.Aluguel
                    && l.Data >= inicioIntervalo
                    && l.Data <= fimIntervalo)
                .Select(l => new { l.ContratoId, l.Data })
                .ToListAsync())
            .ToLookup(l => l.ContratoId!.Value);

        foreach (var (contrato, vencimento) in candidatos)
        {
            var pagoNaCompetencia = lancamentosPorContrato[contrato.Id]
                .Any(l => l.Data.Year == vencimento.Year && l.Data.Month == vencimento.Month);
            if (!pagoNaCompetencia)
            {
                contrato.MarcarInadimplente();
            }
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// O vencimento mais recente que já ocorreu até <paramref name="dataReferencia"/>: o deste
    /// mês, se já chegou, senão o do mês anterior — nunca recalculado "para frente" a partir do
    /// mês corrente, para não perder de vista um vencimento vencido perto da virada do mês (ex.:
    /// `DiaVencimento` 28 em janeiro continua sendo o vencimento relevante em 1º de fevereiro,
    /// até que fevereiro tenha seu próprio vencimento). Retorna null se o Contrato ainda não
    /// existia nesse vencimento (não deve ser avaliado).
    /// </summary>
    private static DateOnly? VencimentoRelevante(Contrato contrato, DateOnly dataReferencia)
    {
        var vencimentoDoMesAtual = VencimentoDoMes(dataReferencia.Year, dataReferencia.Month, contrato.DiaVencimento);
        var vencimento = dataReferencia >= vencimentoDoMesAtual
            ? vencimentoDoMesAtual
            : VencimentoDoMes(dataReferencia.Year, dataReferencia.Month, contrato.DiaVencimento, mesesAtras: 1);

        return vencimento < contrato.DataInicio ? null : vencimento;
    }

    /// <summary>Clampa o dia de vencimento ao último dia do mês (ex.: DiaVencimento 31 em fevereiro).</summary>
    private static DateOnly VencimentoDoMes(int ano, int mes, int diaVencimento, int mesesAtras = 0)
    {
        var referencia = new DateOnly(ano, mes, 1).AddMonths(-mesesAtras);
        var ultimoDiaDoMes = DateTime.DaysInMonth(referencia.Year, referencia.Month);
        return new DateOnly(referencia.Year, referencia.Month, Math.Min(diaVencimento, ultimoDiaDoMes));
    }
}
