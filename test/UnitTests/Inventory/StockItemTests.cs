using Company.Inventory.Domain.Stock;
using Company.Webshop.Shared.Exceptions;

namespace UnitTests.Inventory;

public sealed class StockItemTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReplenishmentMustBePositive(int quantity)
        => Assert.Throws<BusinessRuleException>(() => NewItem().Replenish(quantity, DateTimeOffset.UtcNow));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReservationMustBePositive(int quantity)
        => Assert.Throws<BusinessRuleException>(() => NewItem().TryReserve(quantity, DateTimeOffset.UtcNow));

    [Fact]
    public void SuccessfulReservationReducesStock()
    {
        var item = NewItem(); item.Replenish(10, DateTimeOffset.UtcNow);
        Assert.True(item.TryReserve(4, DateTimeOffset.UtcNow)); Assert.Equal(6, item.AvailableQuantity);
    }

    [Fact]
    public void RejectedReservationLeavesStockUnchanged()
    {
        var item = NewItem(); item.Replenish(2, DateTimeOffset.UtcNow);
        Assert.False(item.TryReserve(5, DateTimeOffset.UtcNow)); Assert.Equal(2, item.AvailableQuantity);
    }

    [Fact]
    public void StockNeverBecomesNegative()
    {
        var item = NewItem(); item.Replenish(1, DateTimeOffset.UtcNow); item.TryReserve(2, DateTimeOffset.UtcNow);
        Assert.True(item.AvailableQuantity >= 0);
    }

    private static StockItem NewItem() => StockItem.Create(Guid.NewGuid(), DateTimeOffset.UtcNow);
}
