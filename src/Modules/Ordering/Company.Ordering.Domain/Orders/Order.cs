using Company.Ordering.Domain.Shared;
using Company.Webshop.Shared.Exceptions;
using Company.Ordering.Domain.Shared.DomainEvents;
using Company.Ordering.Domain.Orders.Events;

namespace Company.Ordering.Domain.Orders;

internal readonly record struct OrderId(Guid Value) : IEntityId
{
    public static OrderId New() => EntityId.New<OrderId>();
    public static OrderId From(Guid value) { Assertions.NotEmpty(value, "Order id is required."); return new(value); }
}
internal readonly record struct OrderLineId(Guid Value) : IEntityId
{
    public static OrderLineId New() => EntityId.New<OrderLineId>();
}
internal sealed class Quantity : ValueObject
{
    private Quantity(int value) => Value = value;
    public int Value { get; }
    public static Quantity From(int value) { Assertions.Positive(value, "Order quantity must be positive."); return new(value); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}
internal sealed class AcceptedPrice : ValueObject
{
    private AcceptedPrice(decimal amount) => Amount = amount;
    public decimal Amount { get; }
    public static AcceptedPrice From(decimal amount) { Assertions.Positive(amount, "Accepted price must be positive."); return new(amount); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Amount; }
}

internal enum OrderStatus { Pending, Confirmed, Rejected }
internal sealed class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderLine> _lines = [];
    private Order() { }
    private Order(OrderId id, Guid productId, Quantity quantity, AcceptedPrice acceptedPrice, DateTimeOffset now) : base(id)
    {
        Status = OrderStatus.Pending; CreatedAt = UpdatedAt = now;
        _lines.Add(OrderLine.Create(OrderLineId.New(), id, productId, quantity, acceptedPrice));
        EnsureValidState();
        Raise(new OrderCreated(id.Value, now));
    }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyCollection<OrderLine> Lines => _lines;
    public static Order Place(OrderId id, Guid productId, Quantity quantity, AcceptedPrice acceptedPrice, DateTimeOffset now)
        => new(id, productId, quantity, acceptedPrice, now);
    public static Order Place(Guid id, Guid productId, int quantity, decimal acceptedPrice, DateTimeOffset now)
        => Place(OrderId.From(id), productId, Quantity.From(quantity), AcceptedPrice.From(acceptedPrice), now);
    public void Confirm(DateTimeOffset now) { RequirePending(); Status = OrderStatus.Confirmed; UpdatedAt = now; Raise(new OrderConfirmed(Id.Value, now)); }
    public void Reject(DateTimeOffset now) { RequirePending(); Status = OrderStatus.Rejected; UpdatedAt = now; Raise(new OrderRejected(Id.Value, now)); }
    private void RequirePending()
    {
        if (Status != OrderStatus.Pending) throw new BusinessRuleException($"Cannot transition an order from {Status}.");
    }
    protected override void ValidateState()
    {
        Assertions.NotEmpty(Id.Value, "Order id is required.");
        if (_lines.Count == 0) throw new BusinessRuleException("An order must contain at least one line.");
    }
}

internal sealed class OrderLine : Entity<OrderLineId>
{
    private OrderLine() { }
    private OrderLine(OrderLineId id, OrderId orderId, Guid productId, Quantity quantity, AcceptedPrice acceptedPrice) : base(id)
    { OrderId = orderId; ProductId = productId; Quantity = quantity; AcceptedPrice = acceptedPrice; EnsureValidState(); }
    public OrderId OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity Quantity { get; private set; } = null!;
    public AcceptedPrice AcceptedPrice { get; private set; } = null!;
    internal static OrderLine Create(OrderLineId id, OrderId orderId, Guid productId, Quantity quantity, AcceptedPrice acceptedPrice)
        => new(id, orderId, productId, quantity, acceptedPrice);
    protected override void ValidateState()
    {
        Assertions.NotEmpty(Id.Value, "Order line id is required.");
        Assertions.NotEmpty(OrderId.Value, "Order id is required.");
        Assertions.NotEmpty(ProductId, "Product id is required.");
    }
}
