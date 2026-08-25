using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PatriHub.Infrastructure.Identity;

namespace PatriHub.Infrastructure.Jwt;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTimeOffset ExpiraEm) GerarToken(ApplicationUser usuario, IEnumerable<string> papeis)
    {
        var expiraEm = DateTimeOffset.UtcNow.AddDays(_options.ExpiraEmDias);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
            new(PatriHubClaimTypes.Nome, usuario.Nome),
        };

        claims.AddRange(papeis.Select(papel => new Claim(ClaimTypes.Role, papel)));

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiraEm.UtcDateTime,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
