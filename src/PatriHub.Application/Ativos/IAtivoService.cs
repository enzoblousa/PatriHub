using PatriHub.Application.Common;

namespace PatriHub.Application.Ativos;

/// <summary>
/// Cadastro, edição, listagem e exclusão (soft delete) de Ativos (Imóvel/Carro). Toda operação
/// recebe o `usuarioId` extraído do JWT e filtra implicitamente por ele — nunca por um
/// parâmetro vindo do cliente (ver 01-SPEC-FUNCIONAL.md §7).
/// </summary>
public interface IAtivoService
{
    Task<ResultadoOperacao<AtivoDetalheDto>> CriarImovelAsync(Guid usuarioId, ImovelRequest request);

    Task<ResultadoOperacao<AtivoDetalheDto>> CriarCarroAsync(Guid usuarioId, CarroRequest request);

    Task<ResultadoOperacao<AtivoDetalheDto>> AtualizarImovelAsync(Guid usuarioId, Guid ativoId, ImovelRequest request);

    Task<ResultadoOperacao<AtivoDetalheDto>> AtualizarCarroAsync(Guid usuarioId, Guid ativoId, CarroRequest request);

    Task<ResultadoOperacao<AtivoDetalheDto>> MarcarStatusAsync(Guid usuarioId, Guid ativoId, MarcarStatusAtivoRequest request);

    Task<ResultadoOperacao> ExcluirAsync(Guid usuarioId, Guid ativoId);

    Task<IReadOnlyList<AtivoResumoDto>> ListarAsync(Guid usuarioId);

    Task<ResultadoOperacao<AtivoDetalheDto>> ObterDetalheAsync(Guid usuarioId, Guid ativoId);
}
