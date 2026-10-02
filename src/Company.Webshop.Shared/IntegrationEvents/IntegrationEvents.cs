using Microsoft.Extensions.DependencyInjection;

namespace Company.Webshop.Shared.IntegrationEvents;

public interface IIntegrationEvent;

public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task Handle(TEvent integrationEvent, CancellationToken cancellationToken);
}

public interface IIntegrationEventDispatcher
{
    Task Publish<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}

internal sealed class InProcessIntegrationEventDispatcher(IServiceScopeFactory scopeFactory)
    : IIntegrationEventDispatcher
{
    public async Task Publish<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IEnumerable<IIntegrationEventHandler<TEvent>> handlers =
            scope.ServiceProvider.GetServices<IIntegrationEventHandler<TEvent>>();

        foreach (IIntegrationEventHandler<TEvent> handler in handlers)
        {
            await handler.Handle(integrationEvent, cancellationToken);
        }
    }
}

public static class IntegrationEventRegistration
{
    public static IServiceCollection AddInProcessIntegrationEvents(this IServiceCollection services)
    {
        services.AddSingleton<IIntegrationEventDispatcher, InProcessIntegrationEventDispatcher>();
        return services;
    }
}
