using System.Net.Http.Json;
using IntegrationTest.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Company.Inventory.Infrastructure;
using Company.Inventory.Domain.Stock;

namespace IntegrationTest.Inventory;

[Collection(IntegrationTestCollection.Name)]
public sealed class InventoryPersistenceTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task StockCanBeReloadedFromANewDbContextScope()
    {
        var id = Guid.NewGuid();
        (await fixture.Client.PostAsJsonAsync($"/inventory/products/{id}/replenish", new { quantity = 8 })).EnsureSuccessStatusCode();
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        Assert.Equal(8, (await db.StockItems.AsNoTracking().SingleAsync(x => x.Id == ProductId.From(id))).AvailableQuantity);
    }
}
