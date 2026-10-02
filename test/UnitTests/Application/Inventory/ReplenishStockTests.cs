using Company.Inventory.Application;
using Company.Inventory.Application.Contracts.Ports;
using Company.Inventory.Domain.Stock;

namespace UnitTests.Application.Inventory;

public sealed class ReplenishStockTests
{
    [Fact]
    public async Task CreatesStockItemAndCommits()
    {
        InMemoryStockItemRepository repository = new InMemoryStockItemRepository();
        RecordingUnitOfWork unitOfWork = new RecordingUnitOfWork();
        ReplenishStock useCase = new ReplenishStock(new InventoryService(repository, unitOfWork));
        Guid productId = Guid.NewGuid();

        await useCase.Execute(new ReplenishStockInput(productId, 10), CancellationToken.None);

        StockItem? stock = await repository.Get(ProductId.From(productId), CancellationToken.None);
        Assert.Equal(10, stock?.AvailableQuantity);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    private sealed class InMemoryStockItemRepository : IStockItemRepository
    {
        private readonly Dictionary<ProductId, StockItem> _items = [];
        private readonly List<StockReservation> _reservations = [];
        public Task<StockItem?> Get(ProductId id, CancellationToken cancellationToken) => Task.FromResult(_items.GetValueOrDefault(id));
        public Task Add(StockItem entity, CancellationToken cancellationToken) { _items.Add(entity.Id, entity); return Task.CompletedTask; }
        public Task<bool> ReservationExists(Guid orderId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AddReservation(StockReservation reservation, CancellationToken cancellationToken) { _reservations.Add(reservation); return Task.CompletedTask; }
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }
        public Task<int> Do(CancellationToken cancellationToken = default) { CommitCount++; return Task.FromResult(1); }
    }
}
