using PatriHub.Application.Common;

namespace PatriHub.Application.Locatarios;

/// <summary>
/// Cadastro, edição, listagem e detalhe de Locatários. Toda operação recebe o `usuarioId`
/// extraído do JWT e filtra implicitamente por ele — nunca por um parâmetro vindo do cliente
/// (ver 01-SPEC-FUNCIONAL.md §7).
/// </summary>
public interface ILocatarioService
{
    Task<ResultadoOperacao<LocatarioDto>> CriarAsync(Guid usuarioId, LocatarioRequest request);

    Task<ResultadoOperacao<LocatarioDto>> AtualizarAsync(Guid usuarioId, Guid locatarioId, LocatarioRequest request);

    Task<ResultadoOperacao<LocatarioDto>> ObterDetalheAsync(Guid usuarioId, Guid locatarioId);

    Task<IReadOnlyList<LocatarioDto>> ListarAsync(Guid usuarioId);
}
