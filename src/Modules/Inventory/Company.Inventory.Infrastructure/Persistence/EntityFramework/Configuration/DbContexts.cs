using Company.Inventory.Application.Contracts.Ports;
using Microsoft.EntityFrameworkCore;

namespace Company.Inventory.Infrastructure.Persistence.EntityFramework.Configuration;

internal abstract class DomainDbContext(DbContextOptions options) : DbContext(options), IUnitOfWork
{
    public Task<int> Do(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);
}

internal abstract class QueryDbContext(DbContextOptions options) : DbContext(options);
internal abstract class PostgresInventoryDomainDbContext(DbContextOptions options) : DomainDbContext(options);

internal sealed class StockItemData
{
    public Guid ProductId { get; init; }
    public int AvailableQuantity { get; init; }
}

internal sealed class PostgresInventoryQueryDbContext(DbContextOptions<PostgresInventoryQueryDbContext> options)
        : QueryDbContext(options)
    {
        public DbSet<StockItemData> StockItems => Set<StockItemData>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StockItemData>(builder =>
            {
                builder.ToTable("stock_items", "inventory");
                builder.HasKey(stock => stock.ProductId);
                builder.Property(stock => stock.ProductId).HasColumnName("product_id");
                builder.Property(stock => stock.AvailableQuantity).HasColumnName("available_quantity");
            });
        }
}
