using System.Net.Http.Json;
using IntegrationTest.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Company.Ordering.Infrastructure;
using Company.Ordering.Domain.Orders;

namespace IntegrationTest.Ordering;

[Collection(IntegrationTestCollection.Name)]
public sealed class OrderingPersistenceTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task AcceptedPriceAndFinalStatusCanBeReloadedFromANewDbContextScope()
    {
        var productId = await fixture.Client.CreatePublishedProduct(44m);
        (await fixture.Client.PostAsJsonAsync($"/inventory/products/{productId}/replenish", new { quantity = 1 })).EnsureSuccessStatusCode();
        var created = await (await fixture.Client.PostAsJsonAsync("/orders", new { productId, quantity = 1 })).ReadRequired<CreatedOrder>();
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var order = await db.Orders.AsNoTracking().Include(x => x.Lines).SingleAsync(x => x.Id == OrderId.From(created.OrderId));
        Assert.Equal("Confirmed", order.Status.ToString()); Assert.Equal(44m, order.Lines.Single().AcceptedPrice.Amount);
    }
}
