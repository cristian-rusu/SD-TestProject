using Company.Ordering.Domain.Shared;

namespace Company.Ordering.Domain.Orders;

internal interface IOrderRepository : IRepository<Order, OrderId>;
