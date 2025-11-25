using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GraphQL;
using GraphQL.Transport;
using GraphQL.Types;
using GraphQL.Validation;
using HintKeep.GraphQL.Definitions.Users;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HintKeep.GraphQL.Functions;

public class GraphQlFunction(
    IHostEnvironment environment,
    ILogger<GraphQlFunction> logger,
    ISchema schema,
    IGraphQLTextSerializer serializer
)
{
    [Function("graphql")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request)
    {
        var response = request.CreateResponse();
        response.Headers.Add("Cache-Control", "no-store");

        try
        {
            if ("post".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
                await HandleGraplQlRequestAsync(request, response);
            else
                await HandleUnsupportedMethodAsync(request, response);
        }
        catch (JsonException jsonException)
        {
            logger.LogError(jsonException, "Invalid JSON in request body.");

            response.StatusCode = HttpStatusCode.BadRequest;
            if (environment.IsDevelopment())
            {
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(serializer.Serialize(jsonException), Encoding.UTF8);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unknown error occurred.");

            response.StatusCode = HttpStatusCode.InternalServerError;
            if (environment.IsDevelopment())
            {
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(serializer.Serialize(exception), Encoding.UTF8);
            }
        }

        return response;
    }

    private async Task HandleGraplQlRequestAsync(HttpRequestData request, HttpResponseData response)
    {
        logger.LogInformation("GraphQL Request Started.");

        string requestBody;
        using (var streamReader = new StreamReader(request.Body))
            requestBody = await streamReader.ReadToEndAsync();
        logger.LogDebug("Executing GraphQL Request '{graphQlRequest}'.", requestBody);

        var graphQlRequest = serializer.Deserialize<GraphQLRequest>(requestBody);

        var result = graphQlRequest is null
            ? "{}"
            : await schema.ExecuteAsync(
                serializer,
                options =>
                {
                    options.User = new ClaimsPrincipal(request.Identities);
                    options.SetHttpCookieHandler(new HintKeepHttp.FunctionsHttpCookieHandler(request, response));

                    options.OperationName = graphQlRequest.OperationName;
                    options.Query = graphQlRequest.Query;
                    options.Variables = graphQlRequest.Variables;
                    options.Extensions = graphQlRequest.Extensions;
                    options.DocumentId = graphQlRequest.DocumentId;

                    options.Root = new object();
                    options.RequestServices = request.FunctionContext.InstanceServices;
                    options.ValidationRules = request.FunctionContext.InstanceServices.GetServices<IValidationRule>();
                }
            );

        logger.LogInformation("GraphQL Request Executed.");

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(result, Encoding.UTF8);

        logger.LogInformation("GraphQL Request Completed.");
    }

    private async Task HandleUnsupportedMethodAsync(HttpRequestData request, HttpResponseData response)
    {
        logger.LogWarning("GraphQL unsupported HTTP method '{HttpMethod}'.", request.Method);
        response.StatusCode = HttpStatusCode.NotFound;
        await response.WriteStringAsync("Method not supported");
    }
}