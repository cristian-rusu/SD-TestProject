using Company.Ordering.Infrastructure.Persistence.EntityFramework.Queries;
using Company.Inventory.Application.Contracts.Public;
using Company.Ordering.Application;
using Company.Ordering.Application.Contracts.Ports;
using Company.Ordering.Domain.Orders;
using Company.Ordering.Infrastructure.Messaging.DomainEvents;
using Company.Ordering.Infrastructure.Persistence.EntityFramework.Configuration;
using Company.Webshop.Shared.IntegrationEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Ordering.Infrastructure;

public static class OrderingModule
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventPublishing();
        services.AddDbContext<OrderingDbContext>((sp, o) => o.UseNpgsql(GetConnection(sp),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ordering"))
            .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));
        services.AddDbContext<PostgresOrderingQueryDbContext>((sp, options) => options.UseNpgsql(GetConnection(sp)));
        services.AddScoped<IQuery<FindOrderByIdInput, OrderDetails?>, FindOrderQuery>();
        services.AddScoped<IOrderRepository, EfCoreOrderRepository>(); services.AddScoped<OrderingService>();
        services.AddScoped<PlaceOrder>(); services.AddScoped<FindOrderById>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<OrderingDbContext>());
        services.AddScoped<IIntegrationEventHandler<StockReserved>, WhenStockReservedConfirmOrder>();
        services.AddScoped<IIntegrationEventHandler<StockReservationRejected>, WhenStockReservationRejectedRejectOrder>();
        return services;
    }

    private static string GetConnection(IServiceProvider services)
        => services.GetRequiredService<IConfiguration>().GetConnectionString("Webshop")
            ?? throw new InvalidOperationException("ConnectionStrings:Webshop is required.");

    public static IEndpointRouteBuilder MapOrderingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/orders").WithTags("Ordering");
        group.MapPost("/", async (PlaceOrderRequest request, PlaceOrder useCase, CancellationToken ct) =>
        { Guid id = await useCase.Execute(new PlaceOrderInput(request.ProductId, request.Quantity), ct); return Results.Created($"/orders/{id}", new { orderId = id }); });
        group.MapGet("/{orderId:guid}", async (Guid orderId, FindOrderById useCase, CancellationToken ct) =>
            await useCase.Execute(new FindOrderByIdInput(orderId), ct) is { } order ? Results.Ok(order) : Results.NotFound());
        return endpoints;
    }

    public static async Task InitializeOrderingDatabase(this IServiceProvider services, CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.MigrateAsync(ct);
    }
}

internal sealed record PlaceOrderRequest(Guid ProductId, int Quantity);
internal sealed class WhenStockReservedConfirmOrder(OrderingService orders) : IIntegrationEventHandler<StockReserved>
{ public Task Handle(StockReserved message, CancellationToken ct) => orders.Confirm(message.OrderId, ct); }
internal sealed class WhenStockReservationRejectedRejectOrder(OrderingService orders) : IIntegrationEventHandler<StockReservationRejected>
{ public Task Handle(StockReservationRejected message, CancellationToken ct) => orders.Reject(message.OrderId, ct); }

