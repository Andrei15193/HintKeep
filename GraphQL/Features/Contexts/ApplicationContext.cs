using System.Net.Http.Json;
using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Transport;

namespace HintKeep.GraphQL.Features.Contexts;

public class ApplicationContext(IGraphQLTextSerializer serializer, HttpClient httpClient)
{
    public UserContext? CurrentUser { get; set; }
    public PageContext PageContext { get; set; } = default!;

    public async Task<JsonNode?> CallGraphAsync(string query, IReadOnlyDictionary<string, object?>? variables = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostAsync(
            "/api/graphql",
            new StringContent(serializer.Serialize(new GraphQLRequest
            {
                Query = query,
                Variables = variables is null ? null : new(
                    variables as IDictionary<string, object?> ?? variables.ToDictionary(variable => variable.Key, variable => variable.Value)
                )
            })),
            cancellationToken
        );

        // The operation succeeded by this point, raising a cancellation when signaled here would cause confusion.
        return await httpResponse.Content.ReadFromJsonAsync<JsonNode>();
    }

    public async Task<JsonNode?> CallGraphFormAsync(string query, CancellationToken cancellationToken = default)
    {
        var formData = PageContext.FormData;
        var formFieldMappings = PageContext.FormFieldMappings;
        var formFieldBackMappings = formFieldMappings.ToDictionary(formFieldMapping => formFieldMapping.Value, formFieldMapping => formFieldMapping.Key);

        var rawResult = await CallGraphAsync(
            query,
            formData.ToDictionary(
                formField => formFieldMappings.TryGetValue(formField.Key, out var mappedKey) ? mappedKey : formField.Key,
                formField => formField.Value
            ),
            cancellationToken
        );

        var dataResult = rawResult!.AsObject()["data"]?.AsObject();

        var errorResult = rawResult.AsObject()["errors"]?.AsArray().FirstOrDefault();
        var fieldsErrorResult = errorResult?.AsObject()["extensions"]?.AsObject()["fields"];

        if (fieldsErrorResult is not null)
            PageContext = PageContext with
            {
                FormErrorData = fieldsErrorResult
                    .AsObject()
                    .ToDictionary(
                        property => formFieldBackMappings.TryGetValue(property.Key, out var mappedKey) ? mappedKey : property.Key,
                        property => property.Value!.GetValue<string>()
                    )
            };
        else if (errorResult is not null)
            throw new Exception($"Unexpected error: {errorResult.ToJsonString()}");

        return dataResult;
    }
}