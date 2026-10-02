using Company.Catalog.Domain.Products;

namespace UnitTests.Catalog;

public sealed class PriceTests
{
    [Fact]
    public void EqualAmountsHaveValueEquality() => Assert.Equal(Price.From(19.95m), Price.From(19.95m));

    [Fact]
    public void DraftCanCarryAnUnresolvedPriceButPublicationCannot()
    {
        var product = Product.Create(ProductId.New(), "Book", string.Empty, Price.From(0), DateTimeOffset.UtcNow);
        Assert.ThrowsAny<Exception>(() => product.Publish(DateTimeOffset.UtcNow));
    }
}
