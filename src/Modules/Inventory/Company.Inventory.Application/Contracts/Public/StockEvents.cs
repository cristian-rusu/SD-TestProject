using Company.Webshop.Shared.IntegrationEvents;

namespace Company.Inventory.Application.Contracts.Public;

public sealed record StockReserved(Guid OrderId, Guid ProductId, int Quantity) : IIntegrationEvent;
public sealed record StockReservationRejected(Guid OrderId, Guid ProductId, int Quantity, string Reason) : IIntegrationEvent;
