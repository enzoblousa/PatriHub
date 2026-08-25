using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Identity;

namespace PatriHub.Infrastructure.Persistence;

public sealed class PatriHubDbContext(DbContextOptions<PatriHubDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Ativo> Ativos => Set<Ativo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.Nome).IsRequired().HasMaxLength(200);
        });

        // Table-per-type: "Ativos" guarda os campos comuns, "Imoveis"/"Carros" guardam os
        // específicos, com PK = FK para Ativos.Id — ver 02-PLANO-TECNICO.md §3.
        builder.Entity<Ativo>(entity =>
        {
            entity.ToTable("Ativos");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Apelido).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.ValorAquisicao).HasPrecision(18, 2);
            entity.Property(a => a.ValorMercadoAtual).HasPrecision(18, 2);
            entity.HasIndex(a => new { a.UsuarioId, a.ExcluidoEm });

            entity.OwnsOne(a => a.Financiamento, fin =>
            {
                fin.Property(f => f.ValorParcela).HasColumnName("Financiamento_ValorParcela").HasPrecision(18, 2);
                fin.Property(f => f.SaldoDevedor).HasColumnName("Financiamento_SaldoDevedor").HasPrecision(18, 2);
                fin.Property(f => f.TaxaJurosAnual).HasColumnName("Financiamento_TaxaJurosAnual").HasPrecision(9, 4);
                fin.Property(f => f.ParcelasRestantes).HasColumnName("Financiamento_ParcelasRestantes");
            });
        });

        builder.Entity<Imovel>(entity =>
        {
            entity.ToTable("Imoveis");
            entity.Property(i => i.TipoImovel).HasConversion<string>().HasMaxLength(20);
            entity.Property(i => i.Matricula).IsRequired().HasMaxLength(100);
            entity.Property(i => i.AreaM2).HasPrecision(10, 2);
            entity.Property(i => i.ValorIptuMensal).HasPrecision(18, 2);
            entity.Property(i => i.ValorCondominioMensal).HasPrecision(18, 2);

            entity.OwnsOne(i => i.Endereco, end =>
            {
                end.Property(e => e.Rua).HasColumnName("Endereco_Rua").IsRequired().HasMaxLength(200);
                end.Property(e => e.Numero).HasColumnName("Endereco_Numero").IsRequired().HasMaxLength(20);
                end.Property(e => e.Complemento).HasColumnName("Endereco_Complemento").HasMaxLength(200);
                end.Property(e => e.Bairro).HasColumnName("Endereco_Bairro").IsRequired().HasMaxLength(100);
                end.Property(e => e.Cidade).HasColumnName("Endereco_Cidade").IsRequired().HasMaxLength(100);
                end.Property(e => e.Uf).HasColumnName("Endereco_Uf").IsRequired().HasMaxLength(2);
                end.Property(e => e.Cep).HasColumnName("Endereco_Cep").IsRequired().HasMaxLength(9);
            });
        });

        builder.Entity<Carro>(entity =>
        {
            entity.ToTable("Carros");
            entity.Property(c => c.Placa).IsRequired().HasMaxLength(10);
            entity.Property(c => c.Marca).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Modelo).IsRequired().HasMaxLength(100);
            entity.Property(c => c.ValorFipeAtual).HasPrecision(18, 2);
            entity.Property(c => c.Km).HasPrecision(10, 1);
            entity.Property(c => c.ConsumoMedio).HasPrecision(6, 2);
        });
    }
}
