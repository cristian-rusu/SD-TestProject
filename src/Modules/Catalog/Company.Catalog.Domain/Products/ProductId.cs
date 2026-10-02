using Company.Catalog.Domain.Shared;

namespace Company.Catalog.Domain.Products;

internal readonly record struct ProductId(Guid Value) : IEntityId
{
    public static ProductId New() => EntityId.New<ProductId>();
    public static ProductId From(Guid value)
    {
        Assertions.NotEmpty(value, "Product id is required.");
        return new ProductId(value);
    }
}
