namespace Company.Catalog.Application.Contracts.Public;

public interface ICatalogModule
{
    Task<ProductForOrdering?> GetProductForOrdering(Guid productId, CancellationToken cancellationToken = default);
}

public sealed record ProductForOrdering(Guid ProductId, decimal Price, bool IsAvailableForSale);
