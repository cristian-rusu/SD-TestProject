using Company.Catalog.Application.Contracts.Public;
using Company.Ordering.Application;
using Company.Ordering.Application.Contracts.Ports;
using Company.Ordering.Application.Contracts.Public;
using Company.Ordering.Domain.Orders;
using Company.Webshop.Shared.IntegrationEvents;

namespace UnitTests.Application.Ordering;

public sealed class PlaceOrderTests
{
    [Fact]
    public async Task StoresAcceptedCatalogPriceBeforePublishingOrderPlaced()
    {
        InMemoryOrderRepository repository = new InMemoryOrderRepository();
        RecordingUnitOfWork unitOfWork = new RecordingUnitOfWork();
        RecordingDispatcher dispatcher = new RecordingDispatcher();
        Guid productId = Guid.NewGuid();
        OrderingService service = new OrderingService(repository, unitOfWork, new CatalogQuerySpy(productId, 23.50m), dispatcher);
        PlaceOrder useCase = new PlaceOrder(service);

        Guid orderId = await useCase.Execute(new PlaceOrderInput(productId, 2), CancellationToken.None);

        Order? order = await repository.Get(OrderId.From(orderId), CancellationToken.None);
        Assert.Equal(23.50m, order?.Lines.Single().AcceptedPrice.Amount);
        Assert.IsType<OrderPlaced>(dispatcher.Published);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    private sealed class CatalogQuerySpy(Guid productId, decimal price) : ICatalogModule
    {
        public Task<ProductForOrdering?> GetProductForOrdering(Guid requestedProductId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ProductForOrdering?>(new ProductForOrdering(productId, price, requestedProductId == productId));
    }

    private sealed class InMemoryOrderRepository : IOrderRepository
    {
        private readonly Dictionary<OrderId, Order> _orders = [];
        public Task<Order?> Get(OrderId id, CancellationToken cancellationToken) => Task.FromResult(_orders.GetValueOrDefault(id));
        public Task Add(Order entity, CancellationToken cancellationToken) { _orders.Add(entity.Id, entity); return Task.CompletedTask; }
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }
        public Task<int> Do(CancellationToken cancellationToken = default) { CommitCount++; return Task.FromResult(1); }
    }

    private sealed class RecordingDispatcher : IIntegrationEventDispatcher
    {
        public IIntegrationEvent? Published { get; private set; }
        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
        {
            Published = integrationEvent;
            return Task.CompletedTask;
        }
    }
}
