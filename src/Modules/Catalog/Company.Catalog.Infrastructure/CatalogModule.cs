using Company.Catalog.Infrastructure.Persistence.EntityFramework.Queries;
using Company.Catalog.Application;
using Company.Catalog.Application.Contracts.Public;
using Company.Catalog.Application.Contracts.Ports;
using Company.Catalog.Domain.Products;
using Company.Catalog.Infrastructure.Messaging.DomainEvents;
using Company.Catalog.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Catalog.Infrastructure;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventPublishing();
        services.AddDbContext<CatalogDbContext>((sp, o) => o.UseNpgsql(GetConnection(sp),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"))
            .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));
        services.AddDbContext<PostgresCatalogQueryDbContext>((sp, options) => options.UseNpgsql(GetConnection(sp)));
        services.AddScoped<IQuery<FindProductByIdInput, ProductDetails?>, FindProductQuery>();
        services.AddScoped<IProductRepository, EfCoreProductRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<CatalogDbContext>());
        services.AddScoped<CatalogService>();
        services.AddScoped<RegisterProduct>(); services.AddScoped<PublishProduct>();
        services.AddScoped<DiscontinueProduct>(); services.AddScoped<FindProductById>();
        services.AddScoped<FindProductForOrdering>();
        services.AddScoped<ICatalogModule>(sp => sp.GetRequiredService<CatalogService>());
        return services;
    }

    private static string GetConnection(IServiceProvider services)
        => services.GetRequiredService<IConfiguration>().GetConnectionString("Webshop")
            ?? throw new InvalidOperationException("ConnectionStrings:Webshop is required.");

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/catalog/products").WithTags("Catalog");
        group.MapPost("/", async (CreateProductRequest request, RegisterProduct useCase, CancellationToken ct) =>
        {
            Guid id = await useCase.Execute(new RegisterProductInput(request.Name, request.Description ?? string.Empty, request.Price), ct);
            return Results.Created($"/catalog/products/{id}", new { productId = id });
        });
        group.MapPost("/{productId:guid}/publish", async (Guid productId, PublishProduct useCase, CancellationToken ct) =>
        { await useCase.Execute(new PublishProductInput(productId), ct); return Results.NoContent(); });
        group.MapPost("/{productId:guid}/discontinue", async (Guid productId, DiscontinueProduct useCase, CancellationToken ct) =>
        { await useCase.Execute(new DiscontinueProductInput(productId), ct); return Results.NoContent(); });
        group.MapGet("/{productId:guid}", async (Guid productId, FindProductById useCase, CancellationToken ct) =>
            await useCase.Execute(new FindProductByIdInput(productId), ct) is { } product ? Results.Ok(product) : Results.NotFound());
        return endpoints;
    }

    public static async Task InitializeCatalogDatabase(this IServiceProvider services, CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync(ct);
    }
}

internal sealed record CreateProductRequest(string Name, string? Description, decimal Price);

