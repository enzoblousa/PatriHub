using PatriHub.Application.Common;

namespace PatriHub.Application.Contratos;

/// <summary>
/// Criação, encerramento, listagem e detalhe de Contratos de locação. Toda operação recebe o
/// `usuarioId` extraído do JWT e filtra implicitamente por ele — nunca por um parâmetro vindo do
/// cliente (ver 01-SPEC-FUNCIONAL.md §7). Criar/encerrar sincronizam automaticamente o Status do
/// Ativo correspondente (ver <see cref="PatriHub.Domain.Entidades.Ativo.MarcarAlugado"/>).
/// </summary>
public interface IContratoService
{
    Task<ResultadoOperacao<ContratoDto>> CriarAsync(Guid usuarioId, ContratoRequest request);

    Task<ResultadoOperacao<ContratoDto>> EncerrarAsync(Guid usuarioId, Guid contratoId);

    Task<ResultadoOperacao<ContratoDto>> ObterDetalheAsync(Guid usuarioId, Guid contratoId);

    Task<IReadOnlyList<ContratoDto>> ListarAsync(Guid usuarioId);
}
