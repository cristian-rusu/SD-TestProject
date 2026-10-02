using Company.Inventory.Domain.Shared;
using Company.Inventory.Domain.Shared.DomainEvents;

namespace Company.Inventory.Domain.Stock;

internal readonly record struct ProductId(Guid Value) : IEntityId
{
    public static ProductId New() => EntityId.New<ProductId>();
    public static ProductId From(Guid value)
    {
        Assertions.NotEmpty(value, "Product id is required.");
        return new(value);
    }
}

internal readonly record struct StockReservationId(Guid Value) : IEntityId
{
    public static StockReservationId New() => EntityId.New<StockReservationId>();
}

internal sealed class Quantity : ValueObject
{
    private Quantity(int value) => Value = value;
    public int Value { get; }
    public static Quantity Positive(int value)
    {
        Assertions.Positive(value, "Quantity must be positive.");
        return new(value);
    }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

internal sealed class StockItem : AggregateRoot<ProductId>
{
    private StockItem() { }
    private StockItem(ProductId productId, DateTimeOffset now) : base(productId) => UpdatedAt = now;
    public int AvailableQuantity { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public static StockItem Create(ProductId productId, DateTimeOffset now) => new(productId, now);
    public static StockItem Create(Guid productId, DateTimeOffset now) => Create(ProductId.From(productId), now);
    public void Replenish(Quantity quantity, DateTimeOffset now)
    {
        AvailableQuantity = checked(AvailableQuantity + quantity.Value); UpdatedAt = now; EnsureValidState();
    }
    public void Replenish(int quantity, DateTimeOffset now) => Replenish(Quantity.Positive(quantity), now);
    public bool TryReserve(Quantity quantity, DateTimeOffset now)
    {
        if (AvailableQuantity < quantity.Value) return false;
        AvailableQuantity -= quantity.Value; UpdatedAt = now; EnsureValidState(); return true;
    }
    public bool TryReserve(int quantity, DateTimeOffset now) => TryReserve(Quantity.Positive(quantity), now);
    protected override void ValidateState()
    {
        Assertions.NotEmpty(Id.Value, "Product id is required.");
        if (AvailableQuantity < 0) throw new InvalidOperationException("Available stock cannot be negative.");
    }
}

internal enum ReservationStatus { Reserved, Rejected }

internal sealed class StockReservation : Entity<StockReservationId>
{
    private StockReservation() { }
    private StockReservation(StockReservationId id, Guid orderId, ProductId productId, Quantity quantity, ReservationStatus status, DateTimeOffset createdAt) : base(id)
    { OrderId = orderId; ProductId = productId; Quantity = quantity; Status = status; CreatedAt = createdAt; EnsureValidState(); }
    public Guid OrderId { get; private set; }
    public ProductId ProductId { get; private set; }
    public Quantity Quantity { get; private set; } = null!;
    public ReservationStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public static StockReservation Create(Guid orderId, ProductId productId, Quantity quantity, bool reserved, DateTimeOffset now)
        => new(StockReservationId.New(), orderId, productId, quantity, reserved ? ReservationStatus.Reserved : ReservationStatus.Rejected, now);
    protected override void ValidateState()
    {
        Assertions.NotEmpty(Id.Value, "Reservation id is required.");
        Assertions.NotEmpty(OrderId, "Order id is required.");
        Assertions.NotEmpty(ProductId.Value, "Product id is required.");
    }
}
