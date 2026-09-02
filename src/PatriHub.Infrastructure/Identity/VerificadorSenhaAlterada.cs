using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Identity;

/// <summary>
/// Invalidação de sessão sem servidor de sessão nem refresh token (ver ADR-0001 e ADR-0009):
/// um JWT emitido antes da última troca de senha (via /redefinir-senha) deixa de ser aceito,
/// checando o `iat` do token (ver JwtTokenGenerator) contra <see cref="ApplicationUser.SenhaAlteradaEm"/>.
/// `SenhaAlteradaEm` fica em cache por <see cref="DuracaoCache"/> pra não bater no banco em toda
/// requisição autenticada — o efeito colateral é até esse tempo de janela pra um token revogado
/// parar de funcionar de fato, aceito porque o pior caso é pequeno perto do ganho de não
/// consultar o Postgres a cada request.
/// </summary>
public sealed class VerificadorSenhaAlterada(PatriHubDbContext db, IMemoryCache cache) : IVerificadorSenhaAlterada
{
    private static readonly TimeSpan DuracaoCache = TimeSpan.FromSeconds(60);

    public async Task<bool> TokenAindaValidoAsync(Guid usuarioId, DateTimeOffset emitidoEm)
    {
        var chaveCache = $"senha-alterada-em:{usuarioId}";
        if (!cache.TryGetValue(chaveCache, out DateTimeOffset? senhaAlteradaEm))
        {
            senhaAlteradaEm = await db.Users
                .Where(u => u.Id == usuarioId)
                .Select(u => u.SenhaAlteradaEm)
                .FirstOrDefaultAsync();
            cache.Set(chaveCache, senhaAlteradaEm, DuracaoCache);
        }

        return senhaAlteradaEm is null || emitidoEm >= senhaAlteradaEm;
    }
}
