using PatriHub.Application.Common;

namespace PatriHub.Application.Autenticacao;

/// <summary>
/// Registro e login de Usuario. Sem refresh token no MVP (ver ADR-0001) — o token emitido
/// tem vida longa (~7 dias).
/// </summary>
public interface IAutenticacaoService
{
    Task<ResultadoAutenticacao> RegistrarAsync(RegistrarUsuarioRequest request);

    Task<ResultadoAutenticacao> LoginAsync(LoginRequest request);

    /// <summary>
    /// Exclusão definitiva (hard delete) da própria conta e de todo o histórico financeiro
    /// associado — ver ADR-0005. `usuarioId` só pode vir do JWT autenticado (mesmo padrão do
    /// `/me`), nunca de um parâmetro vindo do cliente.
    /// </summary>
    Task<ResultadoOperacao> ExcluirContaAsync(Guid usuarioId);
}
