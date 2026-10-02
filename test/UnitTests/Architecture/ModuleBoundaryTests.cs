using System.Reflection;
using System.Runtime.CompilerServices;
using Company.Catalog.Application.Contracts.Public;
using Company.Catalog.Domain.Products;
using Company.Catalog.Infrastructure;
using Company.Inventory.Application.Contracts.Public;
using Company.Inventory.Domain.Stock;
using Company.Inventory.Infrastructure;
using Company.Ordering.Application.Contracts.Public;
using Company.Ordering.Domain.Orders;
using Company.Ordering.Infrastructure;
using Company.Webshop.Shared.IntegrationEvents;
using NetArchTest.Rules;

namespace UnitTests.Architecture;

public sealed class ModuleBoundaryTests
{
    public static TheoryData<string, Assembly, Assembly, Assembly> Modules => new()
    {
        { "Catalog", typeof(Product).Assembly, typeof(ICatalogModule).Assembly, typeof(CatalogModule).Assembly },
        { "Inventory", typeof(StockItem).Assembly, typeof(StockReserved).Assembly, typeof(InventoryModule).Assembly },
        { "Ordering", typeof(Order).Assembly, typeof(OrderPlaced).Assembly, typeof(OrderingModule).Assembly }
    };

    [Theory]
    [MemberData(nameof(Modules))]
    public void OnlyContractsAndCompositionEntryPointArePublic(string module, Assembly domain, Assembly application, Assembly infrastructure)
    {
        Assert.Empty(domain.GetExportedTypes());
        Assert.All(application.GetExportedTypes(), type =>
            Assert.Equal($"Company.{module}.Application.Contracts.Public", type.Namespace));
        Assert.Equal($"Company.{module}.Infrastructure.{module}Module", Assert.Single(infrastructure.GetExportedTypes()).FullName);
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void InternalsAreVisibleOnlyToOwnLayersAndTests(string module, Assembly domain, Assembly application, Assembly infrastructure)
    {
        foreach (Assembly assembly in new[] { domain, application, infrastructure })
        {
            string[] allowed = [ $"Company.{module}.Application", $"Company.{module}.Infrastructure", "UnitTests", "IntegrationTest" ];
            Assert.All(assembly.GetCustomAttributes<InternalsVisibleToAttribute>(), friend => Assert.Contains(friend.AssemblyName, allowed));
        }
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void DependenciesPointInwardAndCrossModuleAccessUsesOnlyPublicContracts(string module, Assembly domain, Assembly application, Assembly infrastructure)
    {
        AssertNoDependencies(domain, [$"Company.{module}.Application", $"Company.{module}.Infrastructure", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore"]);
        AssertNoDependencies(application, [$"Company.{module}.Infrastructure", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore"]);

        foreach (object[] other in Modules)
        {
            if ((string)other[0] == module) continue;
            // Exact foreign implementation types also catch dependencies on handlers in
            // the Application root namespace, where a namespace-prefix rule is too broad.
            string[] forbidden = new[] { (Assembly)other[1], (Assembly)other[2], (Assembly)other[3] }
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.Namespace != $"Company.{other[0]}.Application.Contracts.Public")
                .Select(type => type.FullName!).ToArray();
            foreach (Assembly assembly in new[] { domain, application, infrastructure })
                AssertNoDependencies(assembly, forbidden);
        }
    }

    [Fact]
    public void SharedSupportDoesNotDependOnBusinessModulesOrHost()
        => AssertNoDependencies(typeof(IIntegrationEvent).Assembly,
            ["Company.Catalog", "Company.Inventory", "Company.Ordering", "Company.Webshop.Main"]);

    [Theory]
    [MemberData(nameof(Modules))]
    public void PublicContractsDoNotExposeImplementationTypes(string module, Assembly domain, Assembly application, Assembly infrastructure)
    {
        TestResult result = Types.InAssembly(application)
            .That().ResideInNamespace($"Company.{module}.Application.Contracts.Public")
            .ShouldNot().HaveDependencyOnAny(domain.GetName().Name!, infrastructure.GetName().Name!,
                $"Company.{module}.Application.Contracts.Ports", "Microsoft.EntityFrameworkCore")
            .GetResult();
        Assert.True(result.IsSuccessful);
    }

    private static void AssertNoDependencies(Assembly assembly, string[] forbidden)
    {
        TestResult result = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(forbidden).GetResult();
        Assert.True(result.IsSuccessful, $"Forbidden dependency in {assembly.GetName().Name}: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
