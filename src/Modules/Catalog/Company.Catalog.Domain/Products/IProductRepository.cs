using Company.Catalog.Domain.Shared;

namespace Company.Catalog.Domain.Products;

internal interface IProductRepository : IRepository<Product, ProductId>;
