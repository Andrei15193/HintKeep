using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HintKeep.GraphQL.Middlewares;

public class LoggingMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();

        var correlationId = context.InstanceServices.GetRequiredKeyedService<Guid>(ServiceKeys.CorrelationId);
        var logger = context.InstanceServices.GetRequiredService<ILogger<LoggingMiddleware>>();

        using (logger.BeginScope(new
        {
            context.FunctionId,
            FunctionName = context.FunctionDefinition.Name,
            FunctionEntry = context.FunctionDefinition.EntryPoint,
            CorrelationId = correlationId,
            httpContext?.Request.Headers.UserAgent,
            RequestPath = httpContext?.Request.Path
        }))
            try
            {
                logger.LogInformation("Executing '{functionName}' ('{functionId}') with '{correlationId}' correlation ID.", context.FunctionDefinition.Name, context.FunctionId, correlationId);

                await next(context);

                logger.LogInformation("Executed '{functionName}' ('{functionId}') with '{correlationId}' correlation ID.", context.FunctionDefinition.Name, context.FunctionId, correlationId);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Execution failed '{functionName}' ('{functionId}') with '{correlationId}' correlation ID.", context.FunctionDefinition.Name, context.FunctionId, correlationId);
                throw;
            }
    }
}