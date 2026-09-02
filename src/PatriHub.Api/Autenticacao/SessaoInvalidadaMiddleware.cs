using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PatriHub.Infrastructure.Identity;

namespace PatriHub.Api.Autenticacao;

/// <summary>
/// Roda entre `UseAuthentication` e `UseAuthorization` (ver Program.cs): se a requisição já
/// chegou autenticada, mas o JWT foi emitido antes da última troca de senha do usuário
/// (`SenhaAlteradaEm`), devolve 401 antes mesmo do endpoint rodar — é o que dá o "invalida todas
/// as sessões" da recuperação de senha, já que não existe refresh token/sessão server-side pra
/// revogar de outro jeito (ver ADR-0001 e ADR-0009). O frontend já trata 401 fazendo logout
/// automático (ver `auth-interceptor.ts`), então não precisa de nada especial aqui além do
/// status code.
/// </summary>
public sealed class SessaoInvalidadaMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IVerificadorSenhaAlterada verificador)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var usuarioId = context.User.ObterUsuarioId();
            var emitidoEm = ObterIat(context.User);

            if (!await verificador.TokenAindaValidoAsync(usuarioId, emitidoEm))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(context);
    }

    /// <summary>
    /// Token sem claim `iat` (só pode acontecer com um JWT emitido antes desta feature existir,
    /// já que JwtTokenGenerator sempre inclui a claim hoje) vira `MinValue` de propósito: se o
    /// usuário já tem `SenhaAlteradaEm` setado, um `iat` desconhecido nunca é "depois" dele, e o
    /// token velho é invalidado — mesmo efeito de segurança que um `iat` real e antigo teria.
    /// </summary>
    private static DateTimeOffset ObterIat(ClaimsPrincipal user)
    {
        var iatClaim = user.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
        return iatClaim is not null && long.TryParse(iatClaim, out var unixSeconds)
            ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            : DateTimeOffset.MinValue;
    }
}
