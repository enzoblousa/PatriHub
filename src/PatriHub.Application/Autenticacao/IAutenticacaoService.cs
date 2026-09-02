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

    /// <summary>
    /// "Esqueci minha senha" — ver ADR-0009. Devolve <see cref="TipoErroOperacao.NaoEncontrado"/>
    /// quando o email não existe (decisão consciente de não mascarar isso, ver ADR-0009);
    /// nunca lança se o envio de email falhar silenciosamente por trás — quem chama trata isso
    /// como sucesso mesmo assim, pra não vazar detalhe de infra pro cliente.
    /// </summary>
    Task<ResultadoOperacao> SolicitarRecuperacaoSenhaAsync(SolicitarRecuperacaoSenhaRequest request);

    /// <summary>
    /// Conclui a recuperação: valida o token (opaco, do Identity) e troca a senha. Sucesso aqui
    /// também invalida qualquer sessão JWT emitida antes da troca (checado na Infrastructure —
    /// ver ADR-0009).
    /// </summary>
    Task<ResultadoOperacao> RedefinirSenhaAsync(RedefinirSenhaRequest request);
}
