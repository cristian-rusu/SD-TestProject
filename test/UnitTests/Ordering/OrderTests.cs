using Company.Ordering.Domain.Orders;
using Company.Webshop.Shared.Exceptions;

namespace UnitTests.Ordering;

public sealed class OrderTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void QuantityMustBePositive(int quantity)
        => Assert.Throws<BusinessRuleException>(() => Order.Place(Guid.NewGuid(), Guid.NewGuid(), quantity, 10m, DateTimeOffset.UtcNow));

    [Fact]
    public void NewOrderStartsPending() => Assert.Equal(OrderStatus.Pending, NewOrder().Status);

    [Fact]
    public void AcceptedPriceIsStored() => Assert.Equal(12.34m, NewOrder().Lines.Single().AcceptedPrice.Amount);

    [Fact]
    public void StockReservedConfirmsPendingOrder()
    { var order = NewOrder(); order.Confirm(DateTimeOffset.UtcNow); Assert.Equal(OrderStatus.Confirmed, order.Status); }

    [Fact]
    public void StockReservationRejectedRejectsPendingOrder()
    { var order = NewOrder(); order.Reject(DateTimeOffset.UtcNow); Assert.Equal(OrderStatus.Rejected, order.Status); }

    [Fact]
    public void InvalidStatusTransitionsAreRejected()
    {
        var confirmed = NewOrder(); confirmed.Confirm(DateTimeOffset.UtcNow);
        Assert.Throws<BusinessRuleException>(() => confirmed.Reject(DateTimeOffset.UtcNow));
        var rejected = NewOrder(); rejected.Reject(DateTimeOffset.UtcNow);
        Assert.Throws<BusinessRuleException>(() => rejected.Confirm(DateTimeOffset.UtcNow));
    }

    private static Order NewOrder() => Order.Place(Guid.NewGuid(), Guid.NewGuid(), 2, 12.34m, DateTimeOffset.UtcNow);
}
