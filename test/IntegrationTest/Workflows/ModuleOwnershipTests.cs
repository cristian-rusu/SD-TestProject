using System.Net.Http.Json;
using Company.Catalog.Domain.Products;
using Company.Catalog.Infrastructure;
using Company.Catalog.Infrastructure.Persistence.EntityFramework.Configuration;
using Company.Inventory.Infrastructure;
using Company.Inventory.Infrastructure.Persistence.EntityFramework.Configuration;
using Company.Ordering.Infrastructure;
using Company.Ordering.Infrastructure.Persistence.EntityFramework.Configuration;
using IntegrationTest.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace IntegrationTest.Workflows;

[Collection(IntegrationTestCollection.Name)]
public sealed class ModuleOwnershipTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task AllSixContextsUseOneDatabaseAndEachModuleHasItsOwnMigrationHistory()
    {
        await using AsyncServiceScope scope = fixture.Factory.Services.CreateAsyncScope();
        DbContext[] contexts =
        [
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>(),
            scope.ServiceProvider.GetRequiredService<PostgresCatalogQueryDbContext>(),
            scope.ServiceProvider.GetRequiredService<InventoryDbContext>(),
            scope.ServiceProvider.GetRequiredService<PostgresInventoryQueryDbContext>(),
            scope.ServiceProvider.GetRequiredService<OrderingDbContext>(),
            scope.ServiceProvider.GetRequiredService<PostgresOrderingQueryDbContext>()
        ];
        Assert.All(contexts, context => Assert.Equal(fixture.ConnectionString, context.Database.GetConnectionString()));

        await using NpgsqlConnection connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new NpgsqlCommand("""
            SELECT table_schema
            FROM information_schema.tables
            WHERE table_name = '__EFMigrationsHistory'
            ORDER BY table_schema
            """, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        List<string> schemas = [];
        while (await reader.ReadAsync()) schemas.Add(reader.GetString(0));
        Assert.Equal(new[] { "catalog", "inventory", "ordering" }, schemas);
    }

    [Fact]
    public async Task AcceptedPriceAndStockSurviveNewHostAfterCatalogPriceChanges()
    {
        Guid productId = await fixture.Client.CreatePublishedProduct(19.99m);
        (await fixture.Client.PostAsJsonAsync($"/inventory/products/{productId}/replenish", new { quantity = 10 })).EnsureSuccessStatusCode();
        CreatedOrder created = await (await fixture.Client.PostAsJsonAsync("/orders", new { productId, quantity = 4 })).ReadRequired<CreatedOrder>();

        // Test-only access to Catalog internals: production callers cannot access this aggregate.
        await using (AsyncServiceScope scope = fixture.Factory.Services.CreateAsyncScope())
        {
            CatalogDbContext catalog = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            Product product = await catalog.Products.SingleAsync(item => item.Id == ProductId.From(productId));
            product.ChangePrice(Price.From(24.99m), DateTimeOffset.UtcNow);
            await catalog.SaveChangesAsync();
        }

        await using WebshopApiFactory restartedHost = new WebshopApiFactory(fixture.ConnectionString);
        using HttpClient client = restartedHost.CreateClient();
        ProductView currentProduct = await (await client.GetAsync($"/catalog/products/{productId}")).ReadRequired<ProductView>();
        OrderView originalOrder = await (await client.GetAsync($"/orders/{created.OrderId}")).ReadRequired<OrderView>();
        StockView stock = await (await client.GetAsync($"/inventory/products/{productId}")).ReadRequired<StockView>();
        Assert.Equal(24.99m, currentProduct.Price);
        Assert.Equal(19.99m, Assert.Single(originalOrder.Lines).AcceptedPrice);
        Assert.Equal("Confirmed", originalOrder.Status);
        Assert.Equal(6, stock.AvailableQuantity);

        CreatedOrder rejected = await (await client.PostAsJsonAsync("/orders", new { productId, quantity = 20 })).ReadRequired<CreatedOrder>();
        OrderView rejectedOrder = await (await client.GetAsync($"/orders/{rejected.OrderId}")).ReadRequired<OrderView>();
        StockView remaining = await (await client.GetAsync($"/inventory/products/{productId}")).ReadRequired<StockView>();
        Assert.Equal("Rejected", rejectedOrder.Status);
        Assert.Equal(24.99m, Assert.Single(rejectedOrder.Lines).AcceptedPrice);
        Assert.Equal(6, remaining.AvailableQuantity);
    }
}
