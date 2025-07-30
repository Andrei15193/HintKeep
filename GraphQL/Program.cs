using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL;
using GraphQL.Conversion;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using HintKeep.GraphQL.Definitions;
using HintKeep.GraphQL.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var mutationFieldTypeKey = new object();

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
    if (typeof(FieldType).IsAssignableFrom(type) && type.GetCustomAttribute<MutationFieldAttribute>() is not null)
        builder.Services.AddKeyedSingleton(typeof(FieldType), mutationFieldTypeKey, type);

    var requestHandlerConcreteInterfaces = type
        .ImplementedInterfaces
        .Where(implementedInterface => implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
    foreach (var requestHandlerConcreteInterface in requestHandlerConcreteInterfaces)
        builder.Services.AddScoped(requestHandlerConcreteInterface, type);
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
    .AddSingleton<ISchema>(resolver => {
        var mutaitonGraphObject = new ObjectGraphType();
        foreach (var mutationField in resolver.GetKeyedServices<FieldType>(mutationFieldTypeKey))
            mutaitonGraphObject.AddField(mutationField);

        return new Schema
        {
            Description = "HintKeep GraphQL API",
            NameConverter = CamelCaseNameConverter.Instance,
            Query = resolver.GetRequiredService<QueryGraphDefinition>(),
            Mutation = mutaitonGraphObject
        };
    });

builder
    .Build()
    .Run();