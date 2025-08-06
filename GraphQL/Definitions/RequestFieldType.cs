using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                var environment = context.RequestServices!.GetRequiredService<IHostEnvironment>();
                var logger = context.RequestServices!.GetRequiredService<ILogger<RequestFieldType<TRequest, TResult>>>();

                var validationResults = new List<ValidationResult>();

                try
                {
                    logger.LogInformation("Parsing '{requestTypeName}' request", typeof(TRequest).Name);
                    var input = GetInput(context);

                    logger.LogInformation("Validating '{requestTypeName}' request", typeof(TRequest).Name);
                    var validationContext = new ValidationContext(input, context.RequestServices, default);
                    if (ObjectValidator.TryValidateObject(input, validationContext, validationResults, validateAllProperties: true) && validationResults.Count == 0)
                    {
                        logger.LogInformation("Executing '{requestTypeName}' request", typeof(TRequest).Name);

                        var result = await context
                            .RequestServices
                            !.GetRequiredService<IRequestHandler<TRequest, TResult?>>()
                            .ExecuteAsync(GetInput(context), context.CancellationToken);

                        logger.LogInformation("Executed '{requestTypeName}' request", typeof(TRequest).Name);
                        return result;
                    }
                }
                catch (ValidationException validationException)
                {
                    validationResults.Add(validationException.ValidationResult);
                }
                catch (Exception exception) when (environment.IsDevelopment())
                {
                    logger.LogError(exception, "Failed to execute '{requestTypeName}' request", typeof(TRequest).Name);
                    context.Errors.Add(new ExecutionError(exception.Message, exception));
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Failed to execute '{requestTypeName}' request", typeof(TRequest).Name);
                    context.Errors.Add(new ExecutionError(exception.Message));
                }

                if (validationResults.Count > 0)
                {
                    logger.LogInformation("Invalid '{requestTypeName}' request", typeof(TRequest).Name);
                    logger.LogDebug(
                        "Invalid '{requestTypeName}' request",
                        JsonSerializer.Serialize(validationResults, context.RequestServices!.GetService<JsonSerializerOptions>())
                    );

                    var validationExecutionError = new ExecutionError("Input validation failed")
                    {
                        Extensions = new()
                        {
                            {
                                "fields",
                                new Dictionary<string, string>(
                                    from validationResult in validationResults
                                    from memberName in validationResult.MemberNames
                                    group validationResult.ErrorMessage by memberName into errorMessagesByMemberNames
                                    select new KeyValuePair<string, string>(errorMessagesByMemberNames.Key, errorMessagesByMemberNames.First()),
                                    StringComparer.Ordinal
                                )
                            }
                        }
                    };
                    context.Errors.Add(validationExecutionError);
                }

                return default;
            }
        );
    }

    protected abstract TRequest GetInput(IResolveFieldContext context);
}