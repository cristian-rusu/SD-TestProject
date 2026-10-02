using Company.Catalog.Application;
using Company.Catalog.Application.Contracts.Ports;
using Company.Catalog.Domain.Products;
using Company.Catalog.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Company.Catalog.Infrastructure.Persistence.EntityFramework.Queries;

internal sealed class FindProductQuery(PostgresCatalogQueryDbContext db)
    : IQuery<FindProductByIdInput, ProductDetails?>
{
    public Task<ProductDetails?> Fetch(FindProductByIdInput filter, CancellationToken cancellationToken = default)
    {
        ProductId.From(filter.ProductId);
        return db.Products.AsNoTracking()
            .Where(product => product.ProductId == filter.ProductId)
            .Select(product => new ProductDetails(product.ProductId, product.Name, product.Description, product.Price, product.Status))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
