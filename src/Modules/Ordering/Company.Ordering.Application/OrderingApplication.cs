using Company.Catalog.Application.Contracts.Public;
using Company.Ordering.Application.Contracts.Public;
using Company.Ordering.Application.Contracts.Ports;
using Company.Ordering.Domain.Orders;
using Company.Webshop.Shared.Exceptions;
using Company.Webshop.Shared.IntegrationEvents;

namespace Company.Ordering.Application;

internal sealed record PlaceOrderInput(Guid ProductId, int Quantity);
internal sealed record FindOrderByIdInput(Guid OrderId);

internal sealed class OrderingService(IOrderRepository orders, IUnitOfWork unitOfWork, ICatalogModule catalog, IIntegrationEventDispatcher events)
{
    public async Task<Guid> Place(Guid productId, int quantity, CancellationToken ct)
    {
        if (quantity <= 0) throw new BusinessRuleException("Order quantity must be positive.");
        ProductForOrdering? product = await catalog.GetProductForOrdering(productId, ct);
        if (product is null || !product.IsAvailableForSale)
            throw new BusinessRuleException("Product is not available for sale.");
        Order order = Order.Place(OrderId.New(), productId, Quantity.From(quantity), AcceptedPrice.From(product.Price), DateTimeOffset.UtcNow);
        await orders.Add(order, ct);
        await unitOfWork.Do(ct); // Ordering transaction commits before dispatch starts.
        await events.Publish(new OrderPlaced(order.Id.Value, productId, quantity), ct);
        return order.Id.Value;
    }

    public async Task Confirm(Guid orderId, CancellationToken ct)
    { Order order = await Required(orderId, ct); order.Confirm(DateTimeOffset.UtcNow); await unitOfWork.Do(ct); }
    public async Task Reject(Guid orderId, CancellationToken ct)
    { Order order = await Required(orderId, ct); order.Reject(DateTimeOffset.UtcNow); await unitOfWork.Do(ct); }
    private async Task<Order> Required(Guid id, CancellationToken ct)
        => await orders.Get(OrderId.From(id), ct) ?? throw new KeyNotFoundException($"Order {id} was not found.");

}

internal sealed record OrderDetails(Guid OrderId, string Status, IReadOnlyCollection<OrderLineDetails> Lines);
internal sealed record OrderLineDetails(Guid ProductId, int Quantity, decimal AcceptedPrice);

internal sealed class PlaceOrder(OrderingService ordering) : IUseCase<PlaceOrderInput, Guid>
{ public Task<Guid> Execute(PlaceOrderInput input, CancellationToken cancellationToken) => ordering.Place(input.ProductId, input.Quantity, cancellationToken); }
internal sealed class FindOrderById(IQuery<FindOrderByIdInput, OrderDetails?> query) : IUseCase<FindOrderByIdInput, OrderDetails?>
{ public Task<OrderDetails?> Execute(FindOrderByIdInput input, CancellationToken cancellationToken) => query.Fetch(input, cancellationToken); }
