namespace Company.Catalog.Application.Contracts.Ports;

internal interface IUseCase<in TInput, TOutput>
{
    Task<TOutput> Execute(TInput input, CancellationToken cancellationToken);
}
