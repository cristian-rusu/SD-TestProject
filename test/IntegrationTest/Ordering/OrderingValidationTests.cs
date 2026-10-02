using System.Net;
using System.Net.Http.Json;
using Company.Ordering.Infrastructure;
using IntegrationTest.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTest.Ordering;

[Collection(IntegrationTestCollection.Name)]
public sealed class OrderingValidationTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task UnknownProductCreatesNoOrder()
    {
        var before = await CountOrders();
        Assert.Equal(HttpStatusCode.BadRequest, (await fixture.Client.PostAsJsonAsync("/orders", new { productId = Guid.NewGuid(), quantity = 1 })).StatusCode);
        Assert.Equal(before, await CountOrders());
    }

    [Fact]
    public async Task DraftAndDiscontinuedProductsCannotBeOrdered()
    {
        var before = await CountOrders();
        var draft = await (await fixture.Client.PostAsJsonAsync("/catalog/products", new { name = "Draft", description = "", price = 10m })).ReadRequired<CreatedProduct>();
        Assert.Equal(HttpStatusCode.BadRequest, (await fixture.Client.PostAsJsonAsync("/orders", new { productId = draft.ProductId, quantity = 1 })).StatusCode);

        var discontinued = await fixture.Client.CreatePublishedProduct();
        (await fixture.Client.PostAsync($"/catalog/products/{discontinued}/discontinue", null)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.BadRequest, (await fixture.Client.PostAsJsonAsync("/orders", new { productId = discontinued, quantity = 1 })).StatusCode);
        Assert.Equal(before, await CountOrders());
    }

    [Fact]
    public async Task NonPositiveQuantityIsRejected()
    {
        var before = await CountOrders();
        var product = await fixture.Client.CreatePublishedProduct();
        Assert.Equal(HttpStatusCode.BadRequest, (await fixture.Client.PostAsJsonAsync("/orders", new { productId = product, quantity = 0 })).StatusCode);
        Assert.Equal(before, await CountOrders());
    }

    private async Task<int> CountOrders()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Orders.CountAsync();
    }
}
