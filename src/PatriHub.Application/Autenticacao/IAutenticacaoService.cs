namespace PatriHub.Application.Autenticacao;

/// <summary>
/// Registro e login de Usuario. Sem refresh token no MVP (ver ADR-0001) — o token emitido
/// tem vida longa (~7 dias).
/// </summary>
public interface IAutenticacaoService
{
    Task<ResultadoAutenticacao> RegistrarAsync(RegistrarUsuarioRequest request);

    Task<ResultadoAutenticacao> LoginAsync(LoginRequest request);
}
