using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatriHub.Application.Ativos;
using PatriHub.Application.Autenticacao;
using PatriHub.Infrastructure.Ativos;
using PatriHub.Infrastructure.Identity;
using PatriHub.Infrastructure.Jwt;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Lê a connection string de IConfiguration em tempo de resolução (não no momento deste
        // AddInfrastructure), para respeitar overrides de configuração aplicados depois — como
        // o WebApplicationFactory faz nos testes de integração (seam 2).
        services.AddDbContext<PatriHubDbContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("PatriHubDb")
                ?? throw new InvalidOperationException("ConnectionStrings:PatriHubDb não configurada.");
            options.UseNpgsql(connectionString);
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<PatriHubDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();
        services.AddScoped<IAtivoService, AtivoService>();

        return services;
    }
}
