using Company.Catalog.Domain.Shared;

namespace Company.Catalog.Domain.Products;

internal sealed class ProductName : ValueObject
{
    private ProductName(string value) => Value = value;
    public string Value { get; }
    public static ProductName Create(string value)
    {
        Assertions.NotBlank(value, "Product name is required.");
        return new ProductName(value.Trim());
    }
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
    public override string ToString() => Value;
}
