using System.Net.Http.Json;
using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Transport;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Gherkin.Quick;

namespace HintKeep.GraphQL.Features;

public abstract class HintKeepFeature(HintKeepWebApplicationFactory factory) : Feature, IClassFixture<HintKeepWebApplicationFactory>
{
    protected HttpClient HttpClient { get; } = factory.CreateClient();

    protected async Task<JsonNode> ExecuteQueryAsync(string query, object? variables = null)
    {
        var serializer = factory.Services.GetRequiredService<IGraphQLTextSerializer>();

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

        var result = await httpResponse.Content.ReadFromJsonAsync<JsonNode>();

        return result!;
    }
}