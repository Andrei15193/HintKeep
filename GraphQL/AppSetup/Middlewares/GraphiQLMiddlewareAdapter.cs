using System.Reflection;
using GraphQL.Server.Ui.GraphiQL;
using HintKeep.GraphQL.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HintKeep.GraphQL.AppSetup.Middlewares;

internal class GraphiQLMiddlewareAdapter : IFunctionsWorkerMiddleware
{
    private static readonly string _graphQlFunctionPath = $"/api/{typeof(GraphQlFunction).GetMethod(nameof(GraphQlFunction.Run))!.GetCustomAttribute<FunctionAttribute>()!.Name}";

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();

        var environment = context.InstanceServices.GetRequiredService<IHostEnvironment>();
        var logger = context.InstanceServices.GetRequiredService<ILogger<GraphiQLMiddlewareAdapter>>();

        if (
            httpContext is not null
            && environment.IsDevelopment()
            && HttpMethods.Get.Equals(httpContext.Request.Method, StringComparison.OrdinalIgnoreCase)
            && _graphQlFunctionPath.Equals(httpContext.Request.Path, StringComparison.OrdinalIgnoreCase))
        {              
            logger.LogInformation("GraphQL Playground Request Started.");

            var graphiQLMiddleware = new GraphiQLMiddleware(
                context => Task.CompletedTask,
                new GraphiQLOptions
                {
                    GraphQLEndPoint = _graphQlFunctionPath,
                    GraphQLWsSubscriptions = false
                }
            );
            await graphiQLMiddleware.Invoke(httpContext);

            logger.LogInformation("GraphQL Playground Request Completed.");
        }
        else
            await next(context);
    }
}