using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Data.Tables;
using Azure.Identity;
using GraphQL;
using GraphQL.Conversion;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using HintKeep.GraphQL;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Definitions;
using HintKeep.GraphQL.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var mutatoinFieldTypeKey = new object();

var builder = FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFunctionsWebApplication();

builder
    .Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Request handlers
foreach (var type in typeof(Program).Assembly.DefinedTypes)
{
    var requestHandlerConcreteInterfaces = type
        .ImplementedInterfaces
        .Where(implementedInterface => implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
    foreach (var requestHandlerConcreteInterface in requestHandlerConcreteInterfaces)
        builder.Services.AddScoped(requestHandlerConcreteInterface, type);
}

// GraphQL & Serialization
foreach (var type in typeof(Program).Assembly.DefinedTypes)
{
    if (typeof(IGraphType).IsAssignableFrom(type))
        builder.Services.AddSingleton(type, type);
    if (typeof(FieldType).IsAssignableFrom(type) && type.GetCustomAttribute<MutationFieldAttribute>() is not null)
        builder.Services.AddKeyedSingleton(typeof(FieldType), mutatoinFieldTypeKey, type);
}

builder
    .Services
    .AddSingleton<ISchema>(resolver =>
    {
        var mutaitonGraphObject = new ObjectGraphType
        {
            Name = "HintKeepMutations",
            Description = "Contains all of HintKeep mutations for performing create, update or delete operations."
        };
        foreach (var mutationField in resolver.GetKeyedServices<FieldType>(mutatoinFieldTypeKey))
            mutaitonGraphObject.AddField(mutationField);

        return new Schema
        {
            Description = "HintKeep GraphQL API",
            NameConverter = CamelCaseNameConverter.Instance,
            Query = resolver.GetRequiredService<QueryGraphDefinition>(),
            Mutation = mutaitonGraphObject
        };
    })
    .AddSingleton<IGraphQLTextSerializer, GraphQLSerializer>()
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
    });

// Azure Storage
builder
    .Services
    .AddSingleton<HintKeepTableStorage>()
    .AddSingleton(services =>
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetValue<string>("AzureWebJobsStorage");
        var tableStorageUri = configuration.GetValue<string>("AzureWebJobsStorage__tableServiceUri");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogInformation("Using connection string for TableServiceClient.");
            return new TableServiceClient(connectionString);
        }
        else if (!string.IsNullOrWhiteSpace(tableStorageUri))
        {
            logger.LogInformation("Using managed identity for TableServiceClient.");
            return new TableServiceClient(new Uri(tableStorageUri), new DefaultAzureCredential());
        }
        else
        {
            logger.LogCritical("Neither AzureWebJobsStorage (connection string) nor AzureWebJobsStorage__tableServiceUri (managed identity) have been configured for TableServiceClient.");
            throw new InvalidOperationException("Expected either AzureWebJobsStorage (connection string) or AzureWebJobsStorage__tableServiceUri (managed identity) to be configured");
        }
    });

// Logging
builder
    .UseMiddleware(
        async (context, next) =>
        {
            var correlationId = context.InstanceServices.GetRequiredKeyedService<Guid>(ServiceKeys.CorrelationId);
            var logger = context.InstanceServices.GetRequiredService<ILogger<Program>>();

            using (logger.BeginScope(new
            {
                context.FunctionId,
                FunctionName = context.FunctionDefinition.Name,
                FunctionEntry = context.FunctionDefinition.EntryPoint,
                CorrelationId = correlationId
            }))
                try
                {
                    logger.LogInformation("Executing '{functionName}' ('{functionId}') with '{correlationId}' correlation ID.", context.FunctionDefinition.Name, context.FunctionId, correlationId);
                    await next();
                    logger.LogInformation("Executed '{functionName}' ('{functionId}') with '{correlationId}' correlation ID.", context.FunctionDefinition.Name, context.FunctionId, correlationId);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Execution failed '{functionName}' ('{functionId}') with '{correlationId}' correlation ID.", context.FunctionDefinition.Name, context.FunctionId, correlationId);
                    throw;
                }
        })
    .Services
    .AddKeyedScoped(typeof(Guid), ServiceKeys.CorrelationId, delegate { return Guid.NewGuid(); });


builder
    .Build()
    .Run();