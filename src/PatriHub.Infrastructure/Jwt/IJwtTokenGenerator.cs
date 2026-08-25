using PatriHub.Infrastructure.Identity;

namespace PatriHub.Infrastructure.Jwt;

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiraEm) GerarToken(ApplicationUser usuario, IEnumerable<string> papeis);
}
