using Company.Ordering.Domain.Shared;

namespace Company.Ordering.Domain.Shared.DomainEvents;

internal interface IDomainEvent { DateTimeOffset OccurredAt { get; } }
internal abstract record BaseDomainEvent(DateTimeOffset OccurredAt) : IDomainEvent;
internal interface IDomainEventListener<in TEvent> where TEvent : IDomainEvent { Task Handle(TEvent domainEvent, CancellationToken cancellationToken); }
internal interface IDomainEventPublisher { Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken); }
internal interface IAggregateRoot { IReadOnlyCollection<IDomainEvent> DomainEvents { get; } void ClearDomainEvents(); }
internal abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : struct, IEntityId
{
    private readonly List<IDomainEvent> _domainEvents = [];
    protected AggregateRoot() { }
    protected AggregateRoot(TId id) : base(id) { }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
