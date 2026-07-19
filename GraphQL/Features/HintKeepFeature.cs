using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Transport;
using HintKeep.GraphQL.Definitions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Gherkin.Quick;

namespace HintKeep.GraphQL.Features;

public abstract class HintKeepFeature //: Feature
{
    private readonly HintKeepWebApplicationFactory _factory;

    protected HintKeepFeature()
    {
        _factory = new HintKeepWebApplicationFactory();

        HttpClient = _factory.CreateClient();
    }

    internal HttpClient HttpClient { get; }

    internal JsonNode? RawResult { get; private set; }
    internal JsonNode? DataResult { get; private set; }
    internal JsonNode? ErrorResult { get; private set; }
    internal JsonNode? FieldsErrorResult { get; private set; }

    internal Task DispatchRequestAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var resultType = requestType
            .GetInterfaces()
            .First(@interface => @interface.IsConstructedGenericType && typeof(IRequest<>).IsAssignableFrom(@interface.GetGenericTypeDefinition()))
            .GetGenericArguments()
            .Single();
        var requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
        var executeAsyncMethod = requestHandlerType.GetMethod(nameof(IRequestHandler<IRequest<object>, object>.ExecuteAsync), BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)!;

        var requestHandler = _factory.Services.GetRequiredService(requestHandlerType);

        var valueTaskResult = executeAsyncMethod.Invoke(requestHandler, [request.EnsureValid(), cancellationToken])!;
        var task = (Task)valueTaskResult
            .GetType()
            .GetMethod(nameof(ValueTask<object>.AsTask), BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
            !.Invoke(valueTaskResult, [])!;

        return task;
    }

    internal async Task<JsonNode> ExecuteQueryAsync(string query, object? variables = null, string? dataField = null)
    {
        var serializer = _factory.Services.GetRequiredService<IGraphQLTextSerializer>();

        var httpResponse = await HttpClient.PostAsync(
            "/api/graphql",
            new StringContent(serializer.Serialize(new GraphQLRequest
            {
                Query = query,
                Variables = variables is null ? null : new(
                    variables
                        .GetType()
                        .GetProperties()
                        .Where(property => property.CanRead)
                        .ToDictionary(
                            property => property.Name,
                            property => property.GetValue(variables),
                            StringComparer.OrdinalIgnoreCase
                        )
                )
            }))
        );

        RawResult = await httpResponse.Content.ReadFromJsonAsync<JsonNode>();
        DataResult = RawResult!.AsObject()["data"]?.AsObject().SingleOrDefault(property => string.IsNullOrWhiteSpace(dataField) || property.Key == dataField).Value;

        ErrorResult = RawResult.AsObject()["errors"]?.AsArray().SingleOrDefault();
        FieldsErrorResult = ErrorResult?.AsObject()["extensions"]?.AsObject()["fields"];

        return RawResult;
    }
}