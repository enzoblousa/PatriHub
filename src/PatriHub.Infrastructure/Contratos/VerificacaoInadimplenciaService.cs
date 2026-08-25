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

        var candidatos = contratosAtivos
            .Where(c => ForaDaCarencia(c, dataReferencia))
            .ToList();

        if (candidatos.Count == 0)
        {
            return;
        }

        // Mesmo mês de competência para todos os candidatos, já que todos são avaliados na mesma
        // data de referência — um único SELECT cobre o mês inteiro (mesma estratégia do
        // AtivoService.ListarAsync para o mês do usuário).
        var inicioMes = new DateOnly(dataReferencia.Year, dataReferencia.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);
        var idsCandidatos = candidatos.Select(c => c.Id).ToList();

        var idsComLancamentoNoMes = (await db.Lancamentos
                .Where(l => l.ContratoId != null
                    && idsCandidatos.Contains(l.ContratoId.Value)
                    && l.Tipo == TipoLancamento.Receita
                    && l.Categoria == CategoriaLancamento.Aluguel
                    && l.Data >= inicioMes
                    && l.Data <= fimMes)
                .Select(l => l.ContratoId!.Value)
                .ToListAsync())
            .ToHashSet();

        foreach (var contrato in candidatos.Where(c => !idsComLancamentoNoMes.Contains(c.Id)))
        {
            contrato.MarcarInadimplente();
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Verdadeiro quando o vencimento do mês de <paramref name="dataReferencia"/> já ocorreu
    /// depois do início do Contrato e passou da carência — ou seja, o Contrato já deveria ter
    /// sido pago para este mês de competência.
    /// </summary>
    private static bool ForaDaCarencia(Contrato contrato, DateOnly dataReferencia)
    {
        var vencimentoDoMes = VencimentoDoMes(dataReferencia.Year, dataReferencia.Month, contrato.DiaVencimento);
        if (vencimentoDoMes < contrato.DataInicio)
        {
            return false; // o Contrato ainda não existia no vencimento deste mês
        }

        return dataReferencia > vencimentoDoMes.AddDays(DiasDeCarencia);
    }

    /// <summary>Clampa o dia de vencimento ao último dia do mês (ex.: DiaVencimento 31 em fevereiro).</summary>
    private static DateOnly VencimentoDoMes(int ano, int mes, int diaVencimento)
    {
        var ultimoDiaDoMes = DateTime.DaysInMonth(ano, mes);
        return new DateOnly(ano, mes, Math.Min(diaVencimento, ultimoDiaDoMes));
    }
}
