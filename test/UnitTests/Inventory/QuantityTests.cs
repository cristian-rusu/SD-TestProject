using Company.Inventory.Domain.Stock;
using Company.Webshop.Shared.Exceptions;

namespace UnitTests.Inventory;

public sealed class QuantityTests
{
    [Fact]
    public void EqualValuesHaveValueEquality() => Assert.Equal(Quantity.Positive(3), Quantity.Positive(3));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void QuantityMustBePositive(int value) =>
        Assert.Throws<BusinessRuleException>(() => Quantity.Positive(value));
}
