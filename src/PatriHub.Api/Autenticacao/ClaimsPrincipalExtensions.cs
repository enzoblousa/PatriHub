using System.Security.Claims;

namespace PatriHub.Api.Autenticacao;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extrai o `UsuarioId` das claims do JWT — nunca de um parâmetro vindo do cliente (ver
    /// 01-SPEC-FUNCIONAL.md §7).
    /// </summary>
    public static Guid ObterUsuarioId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
