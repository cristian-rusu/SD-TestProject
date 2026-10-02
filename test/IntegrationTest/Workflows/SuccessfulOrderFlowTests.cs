using System.Net.Http.Json;
using IntegrationTest.Shared;

namespace IntegrationTest.Workflows;

[Collection(IntegrationTestCollection.Name)]
public sealed class SuccessfulOrderFlowTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task PublishedProductWithEnoughStockIsConfirmedAndReducesStock()
    {
        var productId = await fixture.Client.CreatePublishedProduct(27.50m);
        (await fixture.Client.PostAsJsonAsync($"/inventory/products/{productId}/replenish", new { quantity = 10 })).EnsureSuccessStatusCode();

        var created = await (await fixture.Client.PostAsJsonAsync("/orders", new { productId, quantity = 4 })).ReadRequired<CreatedOrder>();

        var order = await (await fixture.Client.GetAsync($"/orders/{created.OrderId}")).ReadRequired<OrderView>();
        var stock = await (await fixture.Client.GetAsync($"/inventory/products/{productId}")).ReadRequired<StockView>();
        Assert.Equal("Confirmed", order.Status);
        Assert.Equal(27.50m, order.Lines.Single().AcceptedPrice);
        Assert.Equal(6, stock.AvailableQuantity);
    }
}
