using Company.Catalog.Domain.Products;
using Company.Catalog.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Company.Catalog.Infrastructure;

internal sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : PostgresCatalogDomainDbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("products"); b.HasKey(x => x.Id); b.Property(x => x.Id).HasColumnName("product_id").HasConversion(x => x.Value, x => new ProductId(x));
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).HasConversion(x => x.Value, x => ProductName.Create(x));
            b.Property(x => x.Description).HasColumnName("description").HasMaxLength(2000);
            b.Property(x => x.CurrentPrice).HasColumnName("price").HasPrecision(18, 2).HasConversion(x => x.Amount, x => Price.From(x));
            b.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
            b.Property(x => x.CreatedAt).HasColumnName("created_at"); b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            b.Ignore(x => x.IsAvailableForSale);
            b.Ignore(x => x.DomainEvents);
        });
    }
}

