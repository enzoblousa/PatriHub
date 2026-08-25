using Microsoft.EntityFrameworkCore;
using PatriHub.Application.Common;
using PatriHub.Application.Lancamentos;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Lancamentos;

/// <summary>
/// Toda consulta filtra por `UsuarioId` diretamente na query (nunca checa dono depois de
/// carregar) — um Lançamento de outro usuário simplesmente não aparece, o que satisfaz o
/// 404 de isolamento por conta (ver 01-SPEC-FUNCIONAL.md §7).
/// </summary>
public sealed class LancamentoService(PatriHubDbContext db) : ILancamentoService
{
    public async Task<ResultadoOperacao<LancamentoDto>> CriarAsync(Guid usuarioId, LancamentoRequest request)
    {
        if (!await AtivoPertenceAoUsuarioAsync(usuarioId, request.AtivoId))
        {
            return ResultadoOperacao<LancamentoDto>.ComErro("Ativo não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        Lancamento? lancamento = null;
        if (!TentarExecutar(() => lancamento = Lancamento.Registrar(
                usuarioId,
                request.AtivoId,
                request.Tipo,
                request.Categoria,
                request.Valor,
                request.Data,
                request.Descricao,
                request.ContratoId),
            out var erro))
        {
            return erro!;
        }

        db.Lancamentos.Add(lancamento!);
        await db.SaveChangesAsync();
        return ResultadoOperacao<LancamentoDto>.ComSucesso(MapearDto(lancamento!));
    }

    public async Task<ResultadoOperacao<LancamentoDto>> AtualizarAsync(Guid usuarioId, Guid lancamentoId, LancamentoRequest request)
    {
        var lancamento = await BuscarLancamentoDoUsuarioAsync(usuarioId, lancamentoId);
        if (lancamento is null)
        {
            return ResultadoOperacao<LancamentoDto>.ComErro("Lançamento não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        // AtivoId não é editável: o Lançamento não "muda de dono" depois de criado. O request
        // ainda o exige (mesmo corpo do POST — ver LancamentoRequest) só para o cliente
        // reafirmar qual Ativo está editando; se vier diferente, é erro de validação.
        if (lancamento.AtivoId != request.AtivoId)
        {
            return ResultadoOperacao<LancamentoDto>.ComErro("Não é possível mover um Lançamento para outro Ativo.", TipoErroOperacao.Validacao);
        }

        if (!TentarExecutar(() => lancamento.Atualizar(
                request.Tipo,
                request.Categoria,
                request.Valor,
                request.Data,
                request.Descricao,
                request.ContratoId),
            out var erro))
        {
            return erro!;
        }

        await db.SaveChangesAsync();
        return ResultadoOperacao<LancamentoDto>.ComSucesso(MapearDto(lancamento));
    }

    public async Task<ResultadoOperacao> ExcluirAsync(Guid usuarioId, Guid lancamentoId)
    {
        var lancamento = await BuscarLancamentoDoUsuarioAsync(usuarioId, lancamentoId);
        if (lancamento is null)
        {
            return ResultadoOperacao.ComErro("Lançamento não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        db.Lancamentos.Remove(lancamento);
        await db.SaveChangesAsync();
        return ResultadoOperacao.ComSucesso();
    }

    public async Task<ResultadoOperacao<LancamentoDto>> ObterDetalheAsync(Guid usuarioId, Guid lancamentoId)
    {
        var lancamento = await BuscarLancamentoDoUsuarioAsync(usuarioId, lancamentoId);
        if (lancamento is null)
        {
            return ResultadoOperacao<LancamentoDto>.ComErro("Lançamento não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        return ResultadoOperacao<LancamentoDto>.ComSucesso(MapearDto(lancamento));
    }

    public async Task<IReadOnlyList<LancamentoDto>> ListarAsync(Guid usuarioId, LancamentoFiltro filtro)
    {
        var query = db.Lancamentos.Where(l => l.UsuarioId == usuarioId);

        if (filtro.AtivoId is { } ativoId)
        {
            query = query.Where(l => l.AtivoId == ativoId);
        }

        if (filtro.DataInicio is { } dataInicio)
        {
            query = query.Where(l => l.Data >= dataInicio);
        }

        if (filtro.DataFim is { } dataFim)
        {
            query = query.Where(l => l.Data <= dataFim);
        }

        if (filtro.Tipo is { } tipo)
        {
            query = query.Where(l => l.Tipo == tipo);
        }

        var lancamentos = await query.OrderByDescending(l => l.Data).ToListAsync();
        return lancamentos.Select(MapearDto).ToList();
    }

    private Task<bool> AtivoPertenceAoUsuarioAsync(Guid usuarioId, Guid ativoId) =>
        db.Ativos.AnyAsync(a => a.Id == ativoId && a.UsuarioId == usuarioId && a.ExcluidoEm == null);

    private Task<Lancamento?> BuscarLancamentoDoUsuarioAsync(Guid usuarioId, Guid lancamentoId) =>
        db.Lancamentos.FirstOrDefaultAsync(l => l.Id == lancamentoId && l.UsuarioId == usuarioId);

    /// <summary>
    /// Roda um registro/edição de domínio (que valida e pode lançar <see cref="ArgumentException"/>)
    /// e converte a exceção num <see cref="TipoErroOperacao.Validacao"/> — usado por todo método
    /// que constrói ou atualiza um Lançamento, para não repetir o mesmo try/catch em cada um.
    /// </summary>
    private static bool TentarExecutar(Action acao, out ResultadoOperacao<LancamentoDto>? erro)
    {
        try
        {
            acao();
            erro = null;
            return true;
        }
        catch (ArgumentException ex)
        {
            erro = ResultadoOperacao<LancamentoDto>.ComErro(ex.Message, TipoErroOperacao.Validacao);
            return false;
        }
    }

    private static LancamentoDto MapearDto(Lancamento lancamento) => new(
        lancamento.Id,
        lancamento.AtivoId,
        lancamento.ContratoId,
        lancamento.Tipo,
        lancamento.Categoria,
        lancamento.Valor,
        lancamento.Data,
        lancamento.Descricao,
        lancamento.CriadoEm,
        lancamento.AtualizadoEm);
}
