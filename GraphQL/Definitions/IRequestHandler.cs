namespace HintKeep.GraphQL.Definitions;

public interface IRequestHandler<TRequest, TResult>
{
    ValueTask<TResult> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}