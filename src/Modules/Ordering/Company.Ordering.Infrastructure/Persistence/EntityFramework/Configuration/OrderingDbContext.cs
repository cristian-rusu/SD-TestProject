using Company.Ordering.Domain.Orders;
using Company.Ordering.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Company.Ordering.Infrastructure;

internal sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : PostgresOrderingDomainDbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ordering");
        modelBuilder.Entity<Order>(b => { b.ToTable("orders"); b.HasKey(x => x.Id); b.Property(x => x.Id).HasColumnName("order_id").HasConversion(x => x.Value, x => new OrderId(x)); b.Property(x => x.Status).HasColumnName("status").HasConversion<string>(); b.Property(x => x.CreatedAt).HasColumnName("created_at"); b.Property(x => x.UpdatedAt).HasColumnName("updated_at"); b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade); b.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field); b.Ignore(x => x.DomainEvents); });
        modelBuilder.Entity<OrderLine>(b => { b.ToTable("order_lines"); b.HasKey(x => x.Id); b.Property(x => x.Id).HasColumnName("order_line_id").HasConversion(x => x.Value, x => new OrderLineId(x)); b.Property(x => x.OrderId).HasColumnName("order_id").HasConversion(x => x.Value, x => new OrderId(x)); b.Property(x => x.ProductId).HasColumnName("product_id"); b.Property(x => x.Quantity).HasColumnName("quantity").HasConversion(x => x.Value, x => Quantity.From(x)); b.Property(x => x.AcceptedPrice).HasColumnName("accepted_price").HasPrecision(18, 2).HasConversion(x => x.Amount, x => AcceptedPrice.From(x)); });
    }
}

