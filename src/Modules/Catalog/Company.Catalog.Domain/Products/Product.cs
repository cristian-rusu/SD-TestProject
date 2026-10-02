using Company.Catalog.Domain.Shared;
using Company.Webshop.Shared.Exceptions;
using Company.Catalog.Domain.Shared.DomainEvents;
using Company.Catalog.Domain.Products.Events;

namespace Company.Catalog.Domain.Products;

internal sealed class Product : AggregateRoot<ProductId>
{
    private Product() { }
    private Product(ProductId id, ProductName name, string description, Price price, DateTimeOffset now) : base(id)
    {
        Name = name; Description = description.Trim(); CurrentPrice = price;
        Status = ProductStatus.Draft; CreatedAt = UpdatedAt = now;
        EnsureValidState();
        Raise(new ProductRegistered(Id.Value, now));
    }

    public ProductName Name { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public Price CurrentPrice { get; private set; } = null!;
    public ProductStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsAvailableForSale => Status == ProductStatus.Published;

    public static Product Create(ProductId id, string name, string description, Price price, DateTimeOffset now)
        => new(id, ProductName.Create(name), description, price, now);
    public static Product Create(ProductName name, string description, Price price)
        => new(EntityId.New<ProductId>(), name, description, price, DateTimeOffset.UtcNow);
    public static Product Create(Guid id, string name, string description, decimal price, DateTimeOffset now)
        => Create(ProductId.From(id), name, description, Price.From(price), now);

    public void ChangePrice(Price price, DateTimeOffset now)
    {
        if (Status == ProductStatus.Published) price.EnsurePositive();
        CurrentPrice = price; UpdatedAt = now; EnsureValidState();
    }

    public void Publish(DateTimeOffset now)
    {
        if (Status != ProductStatus.Draft) throw new BusinessRuleException("Only draft products can be published.");
        CurrentPrice.EnsurePositive();
        Status = ProductStatus.Published; UpdatedAt = now; EnsureValidState();
        Raise(new ProductPublished(Id.Value, now));
    }

    public void Discontinue(DateTimeOffset now)
    {
        if (Status == ProductStatus.Discontinued) throw new BusinessRuleException("Product is already discontinued.");
        Status = ProductStatus.Discontinued; UpdatedAt = now; EnsureValidState();
        Raise(new ProductDiscontinued(Id.Value, now));
    }

    protected override void ValidateState()
    {
        Assertions.NotEmpty(Id.Value, "Product id is required.");
        Assertions.NotBlank(Name.Value, "Product name is required.");
        if (Status == ProductStatus.Published) CurrentPrice.EnsurePositive();
    }
}
