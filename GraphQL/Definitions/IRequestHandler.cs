using System.ComponentModel.DataAnnotations;

namespace HintKeep.GraphQL.Definitions;

public interface IRequest
{
}

public interface IRequest<out TResult> : IRequest
    where TResult : class
{
}

public interface IRequestHandler<in TRequest, TResult>
    where TRequest : class, IRequest<TResult>
    where TResult : class
{
    ValueTask<TResult> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}

public static class RequestExtensions
{
    public static TRequest EnsureValid<TRequest>(this TRequest request)
        where TRequest : IRequest
    {
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, validateAllProperties: true);

        return request;
    }
}