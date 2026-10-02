namespace Company.Inventory.Application.Contracts.Ports;

internal interface IQuery<in TFilter, TData>
{
    Task<TData> Fetch(TFilter filter, CancellationToken cancellationToken = default);
}
