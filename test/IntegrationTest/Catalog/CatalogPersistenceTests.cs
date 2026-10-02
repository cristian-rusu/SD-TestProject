using IntegrationTest.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Company.Catalog.Infrastructure;
using Company.Catalog.Domain.Products;

namespace IntegrationTest.Catalog;

[Collection(IntegrationTestCollection.Name)]
public sealed class CatalogPersistenceTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task ProductCanBeReloadedFromANewDbContextScope()
    {
        var id = await fixture.Client.CreatePublishedProduct(31m);
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var product = await db.Products.AsNoTracking().SingleAsync(x => x.Id == ProductId.From(id));
        Assert.Equal(31m, product.CurrentPrice.Amount); Assert.True(product.IsAvailableForSale);
    }
}
