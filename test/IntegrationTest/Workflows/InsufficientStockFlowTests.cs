using System.Net.Http.Json;
using IntegrationTest.Shared;

namespace IntegrationTest.Workflows;

[Collection(IntegrationTestCollection.Name)]
public sealed class InsufficientStockFlowTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task InsufficientStockRejectsOrderWithoutReducingStock()
    {
        var productId = await fixture.Client.CreatePublishedProduct();
        (await fixture.Client.PostAsJsonAsync($"/inventory/products/{productId}/replenish", new { quantity = 2 })).EnsureSuccessStatusCode();
        var created = await (await fixture.Client.PostAsJsonAsync("/orders", new { productId, quantity = 5 })).ReadRequired<CreatedOrder>();

        var order = await (await fixture.Client.GetAsync($"/orders/{created.OrderId}")).ReadRequired<OrderView>();
        var stock = await (await fixture.Client.GetAsync($"/inventory/products/{productId}")).ReadRequired<StockView>();
        Assert.Equal("Rejected", order.Status);
        Assert.Equal(2, stock.AvailableQuantity);
    }
}
