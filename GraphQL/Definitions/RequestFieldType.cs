using System.ComponentModel.DataAnnotations;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using ObjectValidator = System.ComponentModel.DataAnnotations.Validator;

namespace HintKeep.GraphQL.Definitions;

public abstract class RequestFieldType<TRequest, TResult> : FieldType
    where TRequest : class
{
    public RequestFieldType()
    {
        Resolver = new FuncFieldResolver<TResult>(
            async context =>
            {
                var validationResults = new List<ValidationResult>();

                try
                {
                    var input = GetInput(context);

                    var validationContext = new ValidationContext(input, context.RequestServices, default);
                    if (ObjectValidator.TryValidateObject(input, validationContext, validationResults, validateAllProperties: true) && validationResults.Count == 0)
                        return await context
                            .RequestServices
                            !.GetRequiredService<IRequestHandler<TRequest, TResult?>>()
                            .ExecuteAsync(GetInput(context), context.CancellationToken);
                }
                catch (ValidationException validationException)
                {
                    validationResults.Add(validationException.ValidationResult);
                }
                catch (Exception exception)
                {
                    context.Errors.Add(new ExecutionError(exception.Message, exception));
                }

                var validationExecutionError = new ExecutionError("Input validation failed")
                {
                    Extensions = new()
                    {
                        {
                            "fields",
                            validationResults
                                .SelectMany(
                                    validationResult => validationResult
                                        .MemberNames
                                        .Select(memberName => (MemberName: memberName, validationResult.ErrorMessage))
                                )
                                .GroupBy(pair => pair.MemberName, pair => pair.ErrorMessage, StringComparer.OrdinalIgnoreCase)
                                .ToDictionary(pair => pair.Key, pair => pair.First())
                        }
                    }
                };
                context.Errors.Add(validationExecutionError);
                return default;
            }
        );
    }

    protected abstract TRequest GetInput(IResolveFieldContext context);
}