using Company.Ordering.Application.Contracts.Ports;
using Microsoft.EntityFrameworkCore;

namespace Company.Ordering.Infrastructure.Persistence.EntityFramework.Configuration;

internal abstract class DomainDbContext(DbContextOptions options) : DbContext(options), IUnitOfWork
{
    public Task<int> Do(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);
}

internal abstract class QueryDbContext(DbContextOptions options) : DbContext(options);
internal abstract class PostgresOrderingDomainDbContext(DbContextOptions options) : DomainDbContext(options);

internal sealed class OrderData
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
}

internal sealed class OrderLineData
{
    public Guid OrderLineId { get; init; }
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal AcceptedPrice { get; init; }
}

internal sealed class PostgresOrderingQueryDbContext(DbContextOptions<PostgresOrderingQueryDbContext> options)
        : QueryDbContext(options)
    {
        public DbSet<OrderData> Orders => Set<OrderData>();
        public DbSet<OrderLineData> OrderLines => Set<OrderLineData>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderData>(builder =>
            {
                builder.ToTable("orders", "ordering");
                builder.HasKey(order => order.OrderId);
                builder.Property(order => order.OrderId).HasColumnName("order_id");
                builder.Property(order => order.Status).HasColumnName("status");
            });
            modelBuilder.Entity<OrderLineData>(builder =>
            {
                builder.ToTable("order_lines", "ordering");
                builder.HasKey(line => line.OrderLineId);
                builder.Property(line => line.OrderLineId).HasColumnName("order_line_id");
                builder.Property(line => line.OrderId).HasColumnName("order_id");
                builder.Property(line => line.ProductId).HasColumnName("product_id");
                builder.Property(line => line.Quantity).HasColumnName("quantity");
                builder.Property(line => line.AcceptedPrice).HasColumnName("accepted_price").HasPrecision(18, 2);
            });
        }
}
