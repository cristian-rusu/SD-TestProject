using Company.Catalog.Application.Contracts.Public;
using Company.Catalog.Application.Contracts.Ports;
using Company.Catalog.Domain.Products;

namespace Company.Catalog.Application;

internal sealed record RegisterProductInput(string Name, string Description, decimal Price);
internal sealed record PublishProductInput(Guid ProductId);
internal sealed record DiscontinueProductInput(Guid ProductId);
internal sealed record FindProductByIdInput(Guid ProductId);

internal sealed class CatalogService(IProductRepository products, IUnitOfWork unitOfWork) : ICatalogModule
{
    public async Task<Guid> Create(string name, string description, decimal price, CancellationToken ct)
    {
        Product product = Product.Create(ProductId.New(), name, description, Price.From(price), DateTimeOffset.UtcNow);
        await products.Add(product, ct);
        await unitOfWork.Do(ct);
        return product.Id.Value;
    }

    public async Task Publish(Guid id, CancellationToken ct)
    {
        Product product = await Required(id, ct);
        product.Publish(DateTimeOffset.UtcNow);
        await unitOfWork.Do(ct);
    }

    public async Task Discontinue(Guid id, CancellationToken ct)
    {
        Product product = await Required(id, ct);
        product.Discontinue(DateTimeOffset.UtcNow);
        await unitOfWork.Do(ct);
    }

    public async Task<ProductForOrdering?> GetProductForOrdering(Guid productId, CancellationToken cancellationToken = default)
    {
        Product? product = await products.Get(ProductId.From(productId), cancellationToken);
        return product is null ? null : new(product.Id.Value, product.CurrentPrice.Amount, product.IsAvailableForSale);
    }

    private async Task<Product> Required(Guid id, CancellationToken ct)
        => await products.Get(ProductId.From(id), ct) ?? throw new KeyNotFoundException($"Product {id} was not found.");

}

internal sealed record ProductDetails(Guid ProductId, string Name, string Description, decimal Price, string Status);

internal sealed class RegisterProduct(CatalogService catalog) : IUseCase<RegisterProductInput, Guid>
{ public Task<Guid> Execute(RegisterProductInput input, CancellationToken cancellationToken) => catalog.Create(input.Name, input.Description, input.Price, cancellationToken); }
internal sealed class PublishProduct(CatalogService catalog) : IUseCase<PublishProductInput, bool>
{ public async Task<bool> Execute(PublishProductInput input, CancellationToken cancellationToken) { await catalog.Publish(input.ProductId, cancellationToken); return true; } }
internal sealed class DiscontinueProduct(CatalogService catalog) : IUseCase<DiscontinueProductInput, bool>
{ public async Task<bool> Execute(DiscontinueProductInput input, CancellationToken cancellationToken) { await catalog.Discontinue(input.ProductId, cancellationToken); return true; } }
internal sealed class FindProductById(IQuery<FindProductByIdInput, ProductDetails?> query) : IUseCase<FindProductByIdInput, ProductDetails?>
{ public Task<ProductDetails?> Execute(FindProductByIdInput input, CancellationToken cancellationToken) => query.Fetch(input, cancellationToken); }
internal sealed class FindProductForOrdering(CatalogService catalog) : IUseCase<Guid, ProductForOrdering?>
{ public Task<ProductForOrdering?> Execute(Guid input, CancellationToken cancellationToken) => catalog.GetProductForOrdering(input, cancellationToken); }
