using Company.Inventory.Domain.Stock;
using Company.Inventory.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Company.Inventory.Infrastructure;

internal sealed class EfCoreStockItemRepository(InventoryDbContext db) : IStockItemRepository
{
    public Task<StockItem?> Get(ProductId id, CancellationToken ct) => db.StockItems.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task Add(StockItem item, CancellationToken ct) => await db.StockItems.AddAsync(item, ct);
    public Task<bool> ReservationExists(Guid orderId, CancellationToken ct) => db.StockReservations.AnyAsync(x => x.OrderId == orderId, ct);
    public async Task AddReservation(StockReservation reservation, CancellationToken ct) => await db.StockReservations.AddAsync(reservation, ct);
}
