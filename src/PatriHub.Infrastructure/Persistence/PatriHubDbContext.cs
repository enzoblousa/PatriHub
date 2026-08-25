using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PatriHub.Infrastructure.Identity;

namespace PatriHub.Infrastructure.Persistence;

public sealed class PatriHubDbContext(DbContextOptions<PatriHubDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.Nome).IsRequired().HasMaxLength(200);
        });
    }
}
