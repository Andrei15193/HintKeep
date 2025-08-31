namespace HintKeep.GraphQL.Definitions;

public interface IRequest<out TResult>
{
}

public interface IRequestHandler<in TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    ValueTask<TResult> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}