using Company.Inventory.Domain.Stock;
using Company.Inventory.Application.Contracts.Ports;

namespace Company.Inventory.Application;

internal sealed record ReplenishStockInput(Guid ProductId, int Quantity);
internal sealed record FindStockByProductIdInput(Guid ProductId);

internal sealed class InventoryService(IStockItemRepository stocks, IUnitOfWork unitOfWork)
{
    public async Task Replenish(Guid productId, int quantity, CancellationToken ct)
    {
        ProductId id = ProductId.From(productId);
        StockItem? item = await stocks.Get(id, ct);
        if (item is null)
        {
            item = StockItem.Create(id, DateTimeOffset.UtcNow);
            await stocks.Add(item, ct);
        }
        item.Replenish(Quantity.Positive(quantity), DateTimeOffset.UtcNow);
        await unitOfWork.Do(ct);
    }

    public async Task<bool?> Reserve(Guid orderId, Guid productId, int quantity, CancellationToken ct)
    {
        if (await stocks.ReservationExists(orderId, ct)) return null;
        ProductId id = ProductId.From(productId);
        Quantity requested = Quantity.Positive(quantity);
        StockItem? item = await stocks.Get(id, ct);
        if (item is null)
        {
            item = StockItem.Create(id, DateTimeOffset.UtcNow);
            await stocks.Add(item, ct);
        }
        bool reserved = item.TryReserve(requested, DateTimeOffset.UtcNow);
        await stocks.AddReservation(StockReservation.Create(orderId, id, requested, reserved, DateTimeOffset.UtcNow), ct);
        await unitOfWork.Do(ct);
        return reserved;
    }

}

internal sealed record StockDetails(Guid ProductId, int AvailableQuantity);

internal sealed class ReplenishStock(InventoryService inventory) : IUseCase<ReplenishStockInput, bool>
{ public async Task<bool> Execute(ReplenishStockInput input, CancellationToken cancellationToken) { await inventory.Replenish(input.ProductId, input.Quantity, cancellationToken); return true; } }
internal sealed class FindStockByProductId(IQuery<FindStockByProductIdInput, StockDetails?> query) : IUseCase<FindStockByProductIdInput, StockDetails?>
{ public Task<StockDetails?> Execute(FindStockByProductIdInput input, CancellationToken cancellationToken) => query.Fetch(input, cancellationToken); }
