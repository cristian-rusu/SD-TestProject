using Company.Ordering.Domain.Orders;
using Company.Webshop.Shared.Exceptions;

namespace UnitTests.Ordering;

public sealed class ValueObjectTests
{
    [Fact]
    public void AcceptedPriceHasValueEquality() =>
        Assert.Equal(AcceptedPrice.From(10m), AcceptedPrice.From(10m));

    [Fact]
    public void AcceptedPriceMustBePositive() =>
        Assert.Throws<BusinessRuleException>(() => AcceptedPrice.From(0));
}
