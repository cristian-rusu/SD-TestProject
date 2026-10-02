using Company.Ordering.Domain.Orders;
using Company.Ordering.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Company.Ordering.Infrastructure;

internal sealed class EfCoreOrderRepository(OrderingDbContext db) : IOrderRepository
{
    public async Task Add(Order order, CancellationToken ct) => await db.Orders.AddAsync(order, ct);
    public Task<Order?> Get(OrderId id, CancellationToken ct) => db.Orders.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct);
}
