using Company.Webshop.Shared.IntegrationEvents;

namespace Company.Ordering.Application.Contracts.Public;

public sealed record OrderPlaced(Guid OrderId, Guid ProductId, int Quantity) : IIntegrationEvent;
