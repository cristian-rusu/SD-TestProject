using Company.Ordering.Domain.Shared.DomainEvents;

namespace Company.Ordering.Domain.Orders.Events;

internal sealed record OrderCreated(Guid OrderId, DateTimeOffset OccurredAt) : BaseDomainEvent(OccurredAt);
internal sealed record OrderConfirmed(Guid OrderId, DateTimeOffset OccurredAt) : BaseDomainEvent(OccurredAt);
internal sealed record OrderRejected(Guid OrderId, DateTimeOffset OccurredAt) : BaseDomainEvent(OccurredAt);
