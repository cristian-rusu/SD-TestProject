using Company.Inventory.Domain.Stock;

namespace UnitTests.Architecture.Demonstration;

internal sealed class ForbiddenDependencyFixture
{
    public StockItem? ForbiddenInventoryDomainDependency => null;
}
