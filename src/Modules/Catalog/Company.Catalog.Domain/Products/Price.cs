using Company.Catalog.Domain.Shared;

namespace Company.Catalog.Domain.Products;

internal sealed class Price : ValueObject
{
    private Price(decimal amount) => Amount = amount;
    public decimal Amount { get; }
    public static Price Create(decimal amount) => new Price(amount);
    public static Price From(decimal amount) => Create(amount);
    public void EnsurePositive() => Assertions.Positive(Amount, "A product needs a positive price before publication.");
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }
}
