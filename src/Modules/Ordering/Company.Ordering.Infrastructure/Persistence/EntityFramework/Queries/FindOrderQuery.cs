using Company.Ordering.Application;
using Company.Ordering.Application.Contracts.Ports;
using Company.Ordering.Domain.Orders;
using Company.Ordering.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Company.Ordering.Infrastructure.Persistence.EntityFramework.Queries;

internal sealed class FindOrderQuery(PostgresOrderingQueryDbContext db)
    : IQuery<FindOrderByIdInput, OrderDetails?>
{
    public Task<OrderDetails?> Fetch(FindOrderByIdInput filter, CancellationToken cancellationToken = default)
    {
        OrderId.From(filter.OrderId);
        return db.Orders.AsNoTracking()
            .Where(order => order.OrderId == filter.OrderId)
            .Select(order => new OrderDetails(order.OrderId, order.Status,
                db.OrderLines.Where(line => line.OrderId == order.OrderId)
                    .OrderBy(line => line.OrderLineId)
                    .Select(line => new OrderLineDetails(line.ProductId, line.Quantity, line.AcceptedPrice)).ToArray()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
