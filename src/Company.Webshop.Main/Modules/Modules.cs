using Company.Catalog.Infrastructure;
using Company.Inventory.Infrastructure;
using Company.Ordering.Infrastructure;
using Company.Webshop.Main.Modules.WebApi;
using Company.Webshop.Shared.IntegrationEvents;

namespace Company.Webshop.Main.Modules;

public static class Modules
{
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddCatalogModule(configuration)
            .AddInventoryModule(configuration)
            .AddOrderingModule(configuration)
            .AddInProcessIntegrationEvents()
            .AddWebApiModule();

        return services;
    }

    public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCatalogEndpoints();
        endpoints.MapInventoryEndpoints();
        endpoints.MapOrderingEndpoints();
        return endpoints;
    }

    public static async Task InitializeModuleDatabases(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await services.InitializeCatalogDatabase(cancellationToken);
        await services.InitializeInventoryDatabase(cancellationToken);
        await services.InitializeOrderingDatabase(cancellationToken);
    }
}
