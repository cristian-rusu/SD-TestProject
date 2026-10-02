using Company.Catalog.Domain.Products;
using Company.Catalog.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Company.Catalog.Infrastructure;

internal sealed class EfCoreProductRepository(CatalogDbContext db) : IProductRepository
{
    public async Task Add(Product product, CancellationToken ct) => await db.Products.AddAsync(product, ct);
    public Task<Product?> Get(ProductId id, CancellationToken ct) => db.Products.SingleOrDefaultAsync(x => x.Id == id, ct);
}
