using Company.Inventory.Application;
using Company.Inventory.Application.Contracts.Ports;
using Company.Inventory.Domain.Stock;
using Company.Inventory.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Company.Inventory.Infrastructure.Persistence.EntityFramework.Queries;

internal sealed class FindStockQuery(PostgresInventoryQueryDbContext db)
    : IQuery<FindStockByProductIdInput, StockDetails?>
{
    public Task<StockDetails?> Fetch(FindStockByProductIdInput filter, CancellationToken cancellationToken = default)
    {
        ProductId.From(filter.ProductId);
        return db.StockItems.AsNoTracking()
            .Where(stock => stock.ProductId == filter.ProductId)
            .Select(stock => new StockDetails(stock.ProductId, stock.AvailableQuantity))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
