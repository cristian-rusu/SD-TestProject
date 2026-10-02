using System.Net.Http.Json;

namespace IntegrationTest.Shared;

internal static class HttpClientExtensions
{
    public static async Task<T> ReadRequired<T>(this HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("Response body was empty.");
    }

    public static async Task<Guid> CreatePublishedProduct(this HttpClient client, decimal price = 19.95m)
    {
        var created = await (await client.PostAsJsonAsync("/catalog/products", new { name = "Architecture Book", description = "Demo", price }))
            .ReadRequired<CreatedProduct>();
        (await client.PostAsync($"/catalog/products/{created.ProductId}/publish", null)).EnsureSuccessStatusCode();
        return created.ProductId;
    }
}

internal sealed record CreatedProduct(Guid ProductId);
internal sealed record CreatedOrder(Guid OrderId);
internal sealed record ProductView(Guid ProductId, string Name, string Description, decimal Price, string Status);
internal sealed record StockView(Guid ProductId, int AvailableQuantity);
internal sealed record OrderLineView(Guid ProductId, int Quantity, decimal AcceptedPrice);
internal sealed record OrderView(Guid OrderId, string Status, IReadOnlyCollection<OrderLineView> Lines);
