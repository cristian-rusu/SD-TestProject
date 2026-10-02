using Company.Inventory.Infrastructure.Persistence.EntityFramework.Queries;
using Company.Inventory.Application;
using Company.Inventory.Application.Contracts.Public;
using Company.Inventory.Application.Contracts.Ports;
using Company.Inventory.Domain.Stock;
using Company.Inventory.Infrastructure.Messaging.DomainEvents;
using Company.Inventory.Infrastructure.Persistence.EntityFramework.Configuration;
using Company.Ordering.Application.Contracts.Public;
using Company.Webshop.Shared.IntegrationEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Inventory.Infrastructure;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventPublishing();
        services.AddDbContext<InventoryDbContext>((sp, o) => o.UseNpgsql(GetConnection(sp),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory"))
            .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));
        services.AddDbContext<PostgresInventoryQueryDbContext>((sp, options) => options.UseNpgsql(GetConnection(sp)));
        services.AddScoped<IQuery<FindStockByProductIdInput, StockDetails?>, FindStockQuery>();
        services.AddScoped<IStockItemRepository, EfCoreStockItemRepository>(); services.AddScoped<InventoryService>();
        services.AddScoped<ReplenishStock>(); services.AddScoped<FindStockByProductId>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<InventoryDbContext>());
        services.AddScoped<IIntegrationEventHandler<OrderPlaced>, WhenOrderPlacedReserveStock>();
        return services;
    }

    private static string GetConnection(IServiceProvider services)
        => services.GetRequiredService<IConfiguration>().GetConnectionString("Webshop")
            ?? throw new InvalidOperationException("ConnectionStrings:Webshop is required.");

    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/inventory/products").WithTags("Inventory");
        group.MapPost("/{productId:guid}/replenish", async (Guid productId, ReplenishRequest request, ReplenishStock useCase, CancellationToken ct) =>
        { await useCase.Execute(new ReplenishStockInput(productId, request.Quantity), ct); return Results.NoContent(); });
        group.MapGet("/{productId:guid}", async (Guid productId, FindStockByProductId useCase, CancellationToken ct) =>
            await useCase.Execute(new FindStockByProductIdInput(productId), ct) is { } item ? Results.Ok(item) : Results.NotFound());
        return endpoints;
    }

    public static async Task InitializeInventoryDatabase(this IServiceProvider services, CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.MigrateAsync(ct);
    }
}

internal sealed record ReplenishRequest(int Quantity);

internal sealed class WhenOrderPlacedReserveStock(InventoryService inventory, IIntegrationEventDispatcher events) : IIntegrationEventHandler<OrderPlaced>
{
    public async Task Handle(OrderPlaced message, CancellationToken ct)
    {
        bool? reserved = await inventory.Reserve(message.OrderId, message.ProductId, message.Quantity, ct);
        if (reserved is null) return;
        if (reserved.Value) await events.Publish(new StockReserved(message.OrderId, message.ProductId, message.Quantity), ct);
        else await events.Publish(new StockReservationRejected(message.OrderId, message.ProductId, message.Quantity, "Insufficient stock."), ct);
    }
}

