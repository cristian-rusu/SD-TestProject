using Company.Catalog.Domain.Products;
using Company.Webshop.Shared.Exceptions;

namespace UnitTests.Catalog;

public sealed class ProductTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CannotPublishProductWithInvalidPrice(decimal price)
    {
        var product = Product.Create(Guid.NewGuid(), "Book", "Description", price, DateTimeOffset.UtcNow);
        Assert.Throws<BusinessRuleException>(() => product.Publish(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CanPublishProductWithPositivePrice()
    {
        var product = Product.Create(Guid.NewGuid(), "Book", "Description", 12.50m, DateTimeOffset.UtcNow);
        product.Publish(DateTimeOffset.UtcNow);
        Assert.True(product.IsAvailableForSale);
    }

    [Fact]
    public void DraftProductIsNotSellable()
        => Assert.False(Product.Create(Guid.NewGuid(), "Book", "", 10m, DateTimeOffset.UtcNow).IsAvailableForSale);

    [Fact]
    public void DiscontinuedProductIsNotSellable()
    {
        var product = Product.Create(Guid.NewGuid(), "Book", "", 10m, DateTimeOffset.UtcNow);
        product.Publish(DateTimeOffset.UtcNow); product.Discontinue(DateTimeOffset.UtcNow);
        Assert.False(product.IsAvailableForSale);
    }
}
