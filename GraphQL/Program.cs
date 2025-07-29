using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using HintKeep.GraphQL.Definitions;
using HintKeep.GraphQL.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFunctionsWebApplication();

builder
    .Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

foreach (var type in typeof(Program).Assembly.DefinedTypes)
{
    if (typeof(IGraphType).IsAssignableFrom(type))
        builder.Services.AddSingleton(type, type);
}

builder
    .Services
    .AddSingleton(new JsonSerializerOptions
    {
        WriteIndented = builder.Environment.IsDevelopment(),
        IndentSize = 4,
        IndentCharacter = ' ',
        NewLine = "\n",

        RespectNullableAnnotations = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,

        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        IncludeFields = false,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = false,

        AllowTrailingCommas = false,
        PropertyNameCaseInsensitive = false,
        NumberHandling = JsonNumberHandling.Strict,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false),
            new UtcDateTimeConverter()
        }
    })
    .AddSingleton<IGraphQLTextSerializer, GraphQLSerializer>()
    .AddSingleton<ISchema>(resolver => new Schema
    {
        Query = resolver.GetRequiredService<QueryGraphDefinition>()
    });

builder
    .Build()
    .Run();