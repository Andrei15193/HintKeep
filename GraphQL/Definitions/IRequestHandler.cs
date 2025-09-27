using System.ComponentModel.DataAnnotations;

namespace HintKeep.GraphQL.Definitions;


/// <summary>
/// A base marker interface for extending requests at a general level.
/// </summary>
public interface IRequest
{
}


/// <summary>
/// Marker interface for a request that produces a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The result type. Must be a reference type.</typeparam>
/// <remarks>
/// <para>
/// The <c>class</c> constraint is set to avoid accidental boxing when requests or results would be described using value types (<c>struct</c> or <c>record struct</c>) when validated (using <see cref="Validator"/>) or serialized (using <see cref="System.Text.Json.JsonSerializer"/>).
/// </para>
/// <para>
/// The <typeparamref name="TResult"/> is required to strictly map a request and its expected result making <see cref="IRequestHandler{TRequest, TResult}"/> lookups safer.
/// </para>
/// </remarks>
public interface IRequest<out TResult> : IRequest
    where TResult : class
{
}


/// <summary>
/// Handles a request of type <typeparamref name="TRequest"/> and produces a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TRequest">The request type. Must be a reference type implementing <see cref="IRequest{TResult}"/>.</typeparam>
/// <typeparam name="TResult">The result type. Must be a reference type.</typeparam>
/// <remarks>
/// The <c>class</c> constraint is set to avoid accidental boxing when requests or results would be described using value types (<c>struct</c> or <c>record struct</c>) when validated (using <see cref="Validator"/>) or serialized (using <see cref="System.Text.Json.JsonSerializer"/>).
/// </remarks>
public interface IRequestHandler<in TRequest, TResult>
    where TRequest : class, IRequest<TResult>
    where TResult : class
{
    /// <summary>
    /// Executes the request asynchronously.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">A token used to signal cancellation.</param>
    /// <returns>The result of the request.</returns>
    ValueTask<TResult> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}


/// <summary>
/// <see cref="IRequest"/> extension methods for simplifying common operations.
/// </summary>
public static class RequestExtensions
{
    /// <summary>
    /// Validates the request using <see cref="System.ComponentModel.DataAnnotations"/>.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="request">The request instance to validate.</param>
    /// <returns>The validated request instance when valid.</returns>
    /// <exception cref="ValidationException">Thrown if validation fails.</exception>
    public static TRequest EnsureValid<TRequest>(this TRequest request)
        where TRequest : IRequest
    {
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, validateAllProperties: true);

        return request;
    }
}