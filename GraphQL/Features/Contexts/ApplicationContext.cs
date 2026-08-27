using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using GraphQL;
using GraphQL.Transport;

namespace HintKeep.GraphQL.Features.Contexts;

public class ApplicationContext(IGraphQLTextSerializer serializer, HttpClient httpClient)
{
    public UserContext? CurrentUser { get; set; }
    public PageContext PageContext { get; set; } = default!;

    public async Task<JsonNode?> CallGraphAsync(string query, IReadOnlyDictionary<string, object?>? variables = null)
    {
        var httpResponse = await httpClient.PostAsync(
            "/api/graphql",
            new StringContent(serializer.Serialize(new GraphQLRequest
            {
                Query = query,
                Variables = variables is null ? null : new(
                    variables.ToDictionary(
                        variable => Regex.Replace(variable.Key, @"\s+\S", match => match.Value.Trim().ToUpperInvariant()),
                        variable => variable.Value
                    )
                )
            }))
        );

        var rawResult = await httpResponse.Content.ReadFromJsonAsync<JsonNode>();
        var dataResult = rawResult!.AsObject()["data"]?.AsObject();

        var errorResult = rawResult.AsObject()["errors"]?.AsArray().FirstOrDefault();
        var fieldsErrorResult = errorResult?.AsObject()["extensions"]?.AsObject()["fields"];

        if (fieldsErrorResult is not null)
            PageContext = PageContext with
            {
                FormErrorData = fieldsErrorResult
                    .AsObject()
                    .ToDictionary(
                        property => property.Key,
                        property => property.Value!.GetValue<string>()
                    )
            };
        else if (errorResult is not null)
            throw new Exception($"Unexpected error: {errorResult.ToJsonString()}");

        return dataResult;
    }
}