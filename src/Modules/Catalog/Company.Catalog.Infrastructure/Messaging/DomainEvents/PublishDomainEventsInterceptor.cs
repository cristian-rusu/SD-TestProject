using Company.Catalog.Domain.Shared.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Catalog.Infrastructure.Messaging.DomainEvents;

internal sealed class DomainEventPublisher(IServiceProvider services) : IDomainEventPublisher
{
    public async Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Type listenerType = typeof(IDomainEventListener<>).MakeGenericType(domainEvent.GetType());
        IEnumerable<object?> listeners = services.GetServices(listenerType);
        foreach (object? listener in listeners)
        {
            if (listener is null)
            {
                continue;
            }

            System.Reflection.MethodInfo method = listenerType.GetMethod(nameof(IDomainEventListener<IDomainEvent>.Handle))
                ?? throw new InvalidOperationException("Domain event handler method was not found.");
            Task invocation = (Task)(method.Invoke(listener, [domainEvent, cancellationToken])
                ?? throw new InvalidOperationException("Domain event handler returned no task."));
            await invocation;
        }
    }
}

internal sealed class PublishDomainEventsInterceptor(IDomainEventPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return result;
        }

        IAggregateRoot[] aggregates = eventData.Context.ChangeTracker.Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToArray();

        foreach (IAggregateRoot aggregate in aggregates)
        {
            foreach (IDomainEvent domainEvent in aggregate.DomainEvents.ToArray())
            {
                await publisher.Publish(domainEvent, cancellationToken);
            }
            aggregate.ClearDomainEvents();
        }

        return result;
    }
}

internal static class DomainEventRegistration
{
    public static IServiceCollection AddDomainEventPublishing(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        services.AddScoped<PublishDomainEventsInterceptor>();
        return services;
    }
}

