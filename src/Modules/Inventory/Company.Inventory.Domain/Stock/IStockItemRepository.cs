using Company.Inventory.Domain.Shared;

namespace Company.Inventory.Domain.Stock;

internal interface IStockItemRepository : IRepository<StockItem, ProductId>
{
    Task<bool> ReservationExists(Guid orderId, CancellationToken cancellationToken);
    Task AddReservation(StockReservation reservation, CancellationToken cancellationToken);
}
