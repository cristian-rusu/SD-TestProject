using System.Reflection;
using Company.Catalog.Application.Contracts.Public;
using Company.Catalog.Domain.Products;
using Company.Catalog.Infrastructure;
using Company.Inventory.Application.Contracts.Public;
using Company.Inventory.Domain.Stock;
using Company.Inventory.Infrastructure;
using Company.Ordering.Application.Contracts.Public;
using Company.Ordering.Domain.Orders;
using Company.Ordering.Infrastructure;
using NetArchTest.Rules;

namespace UnitTests.Architecture;

public sealed class ArchitectureTests
{
    [Theory]
    [MemberData(nameof(ForbiddenDependencies))]
    public void ProductionAssemblyDoesNotDependOnForbiddenNamespaces(Assembly assembly, string[] forbidden)
    {
        var result = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(forbidden).GetResult();
        Assert.True(result.IsSuccessful, $"{assembly.GetName().Name} has a forbidden dependency: {string.Join(", ", forbidden)}");
    }

    public static TheoryData<Assembly, string[]> ForbiddenDependencies => new()
    {
        { typeof(Product).Assembly, ["Company.Inventory", "Company.Ordering", "Company.Webshop.Main"] },
        { typeof(ICatalogModule).Assembly, ["Company.Inventory", "Company.Ordering", "Company.Webshop.Main"] },
        { typeof(CatalogModule).Assembly, ["Company.Inventory", "Company.Ordering", "Company.Webshop.Main"] },
        { typeof(StockItem).Assembly, ["Company.Catalog", "Company.Ordering", "Company.Webshop.Main"] },
        { typeof(StockReserved).Assembly, ["Company.Catalog", "Company.Ordering.Domain", "Company.Ordering.Infrastructure", "Company.Webshop.Main"] },
        { typeof(InventoryModule).Assembly, ["Company.Catalog.Domain", "Company.Catalog.Infrastructure", "Company.Ordering.Domain", "Company.Ordering.Infrastructure", "Company.Webshop.Main"] },
        { typeof(Order).Assembly, ["Company.Catalog", "Company.Inventory", "Company.Webshop.Main"] },
        { typeof(OrderPlaced).Assembly, ["Company.Catalog.Domain", "Company.Catalog.Infrastructure", "Company.Inventory.Domain", "Company.Inventory.Infrastructure", "Company.Webshop.Main"] },
        { typeof(OrderingModule).Assembly, ["Company.Catalog.Domain", "Company.Catalog.Infrastructure", "Company.Inventory.Domain", "Company.Inventory.Infrastructure", "Company.Webshop.Main"] }
    };

    [Fact]
    public void ControlledForbiddenDependencyFixtureIsDetected()
    {
        var result = Types.InAssembly(typeof(Demonstration.ForbiddenDependencyFixture).Assembly)
            .That().ResideInNamespace("UnitTests.Architecture.Demonstration")
            .ShouldNot().HaveDependencyOn("Company.Inventory.Domain")
            .GetResult();
        Assert.False(result.IsSuccessful);
    }
}
