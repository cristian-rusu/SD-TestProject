using Company.Catalog.Domain.Shared.DomainEvents;

namespace Company.Catalog.Domain.Products.Events;

internal sealed record ProductRegistered(Guid ProductId, DateTimeOffset OccurredAt) : BaseDomainEvent(OccurredAt);
internal sealed record ProductPublished(Guid ProductId, DateTimeOffset OccurredAt) : BaseDomainEvent(OccurredAt);
internal sealed record ProductDiscontinued(Guid ProductId, DateTimeOffset OccurredAt) : BaseDomainEvent(OccurredAt);
