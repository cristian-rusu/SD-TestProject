using Company.Catalog.Application.Contracts.Ports;
using Microsoft.EntityFrameworkCore;

namespace Company.Catalog.Infrastructure.Persistence.EntityFramework.Configuration;

internal abstract class DomainDbContext(DbContextOptions options) : DbContext(options), IUnitOfWork
{
    public Task<int> Do(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);
}

internal abstract class QueryDbContext(DbContextOptions options) : DbContext(options);
internal abstract class PostgresCatalogDomainDbContext(DbContextOptions options) : DomainDbContext(options);

internal sealed class CatalogProductData
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Status { get; init; } = string.Empty;
}

internal sealed class PostgresCatalogQueryDbContext(DbContextOptions<PostgresCatalogQueryDbContext> options)
        : QueryDbContext(options)
    {
        public DbSet<CatalogProductData> Products => Set<CatalogProductData>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatalogProductData>(builder =>
            {
                builder.ToTable("products", "catalog");
                builder.HasKey(product => product.ProductId);
                builder.Property(product => product.ProductId).HasColumnName("product_id");
                builder.Property(product => product.Name).HasColumnName("name");
                builder.Property(product => product.Description).HasColumnName("description");
                builder.Property(product => product.Price).HasColumnName("price");
                builder.Property(product => product.Status).HasColumnName("status");
            });
        }
}
