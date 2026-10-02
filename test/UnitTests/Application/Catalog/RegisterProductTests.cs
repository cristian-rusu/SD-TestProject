using Company.Catalog.Application;
using Company.Catalog.Application.Contracts.Ports;
using Company.Catalog.Domain.Products;

namespace UnitTests.Application.Catalog;

public sealed class RegisterProductTests
{
    [Fact]
    public async Task SavesDraftProductAndCommits()
    {
        InMemoryProductRepository repository = new InMemoryProductRepository();
        RecordingUnitOfWork unitOfWork = new RecordingUnitOfWork();
        RegisterProduct useCase = new RegisterProduct(new CatalogService(repository, unitOfWork));

        Guid productId = await useCase.Execute(new RegisterProductInput("Book", "DDD", 19.99m), CancellationToken.None);

        Product? product = await repository.Get(ProductId.From(productId), CancellationToken.None);
        Assert.NotNull(product);
        Assert.Equal(ProductStatus.Draft, product.Status);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    private sealed class InMemoryProductRepository : IProductRepository
    {
        private readonly Dictionary<ProductId, Product> _products = [];
        public Task<Product?> Get(ProductId id, CancellationToken cancellationToken) =>
            Task.FromResult(_products.GetValueOrDefault(id));
        public Task Add(Product entity, CancellationToken cancellationToken)
        {
            _products.Add(entity.Id, entity);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }
        public Task<int> Do(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(1);
        }
    }
}
