namespace Company.Catalog.Application.Contracts.Ports;

internal interface IUnitOfWork
{
    Task<int> Do(CancellationToken cancellationToken = default);
}
