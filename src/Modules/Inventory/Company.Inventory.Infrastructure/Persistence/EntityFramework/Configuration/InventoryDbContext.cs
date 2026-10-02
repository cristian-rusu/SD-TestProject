using Company.Inventory.Domain.Stock;
using Company.Inventory.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Company.Inventory.Infrastructure;

internal sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : PostgresInventoryDomainDbContext(options)
{
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");
        modelBuilder.Entity<StockItem>(b => { b.ToTable("stock_items"); b.HasKey(x => x.Id); b.Property(x => x.Id).HasColumnName("product_id").HasConversion(x => x.Value, x => new ProductId(x)); b.Property(x => x.AvailableQuantity).HasColumnName("available_quantity"); b.Property(x => x.UpdatedAt).HasColumnName("updated_at"); b.Ignore(x => x.DomainEvents); });
        modelBuilder.Entity<StockReservation>(b => { b.ToTable("stock_reservations"); b.HasKey(x => x.Id); b.Property(x => x.Id).HasColumnName("reservation_id").HasConversion(x => x.Value, x => new StockReservationId(x)); b.Property(x => x.OrderId).HasColumnName("order_id"); b.HasIndex(x => x.OrderId).IsUnique(); b.Property(x => x.ProductId).HasColumnName("product_id").HasConversion(x => x.Value, x => new ProductId(x)); b.HasOne<StockItem>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict); b.Property(x => x.Quantity).HasColumnName("quantity").HasConversion(x => x.Value, x => Quantity.Positive(x)); b.Property(x => x.Status).HasColumnName("status").HasConversion<string>(); b.Property(x => x.CreatedAt).HasColumnName("created_at"); });
    }
}

