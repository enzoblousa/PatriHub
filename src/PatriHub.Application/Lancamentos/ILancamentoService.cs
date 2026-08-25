using PatriHub.Application.Common;

namespace PatriHub.Application.Lancamentos;

/// <summary>
/// Cadastro, edição, exclusão e listagem/filtro de Lançamentos financeiros. Toda operação
/// recebe o `usuarioId` extraído do JWT e filtra implicitamente por ele — nunca por um
/// parâmetro vindo do cliente (ver 01-SPEC-FUNCIONAL.md §7).
/// </summary>
public interface ILancamentoService
{
    Task<ResultadoOperacao<LancamentoDto>> CriarAsync(Guid usuarioId, LancamentoRequest request);

    Task<ResultadoOperacao<LancamentoDto>> AtualizarAsync(Guid usuarioId, Guid lancamentoId, LancamentoRequest request);

    Task<ResultadoOperacao> ExcluirAsync(Guid usuarioId, Guid lancamentoId);

    Task<ResultadoOperacao<LancamentoDto>> ObterDetalheAsync(Guid usuarioId, Guid lancamentoId);

    /// <summary>
    /// Filtra sempre por `usuarioId`; um `AtivoId` de outro usuário simplesmente não bate com
    /// nenhum lançamento (mesma semântica de isolamento do <see cref="ResultadoOperacao"/> —
    /// aqui sem 404 porque é uma listagem, que apenas fica vazia).
    /// </summary>
    Task<IReadOnlyList<LancamentoDto>> ListarAsync(Guid usuarioId, LancamentoFiltro filtro);
}
