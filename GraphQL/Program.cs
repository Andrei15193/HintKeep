using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Data.Tables;
using Azure.Identity;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Conversion;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using GraphQL.Validation;
using HintKeep.GraphQL;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Definitions;
using HintKeep.GraphQL.Definitions.Users;
using HintKeep.GraphQL.Json;
using HintKeep.GraphQL.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var mutatoinFieldTypeKey = new object();

var builder = FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFunctionsWebApplication();

builder
    .Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Request handlers
var requestHandlers =
    from type in typeof(Program).Assembly.DefinedTypes
    where type.IsClass
    from implementedInterface in type.ImplementedInterfaces
    where implementedInterface.IsConstructedGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
    select (RequestHandlerInterface: implementedInterface, RequestHandlerImplementation: type);

foreach (var (requestHandlerInterface, requestHandlerImplementation) in requestHandlers)
    builder.Services.AddScoped(requestHandlerInterface, requestHandlerImplementation);

// Authentication (JSON Web Tokens)
builder
    .UseMiddleware<AuthenticationMiddleware>()
    .Services
    .AddSingleton<SecurityKey>(services =>
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var signingKey = configuration.GetValue<string>("HINTKEEP_SIGNING_KEY");

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            logger.LogCritical("HINTKEEP_SIGNING_KEY has not been configured.");
            throw new InvalidOperationException("Expected HINTKEEP_SIGNING_KEY to be configured.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
    })
    .AddSingleton<SigningCredentials>(services =>
    {
        return new SigningCredentials(services.GetRequiredService<SecurityKey>(), SecurityAlgorithms.HmacSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
        };
    })
    .AddKeyedSingleton<SigningCredentials>(ServiceKeys.SessionTokenSigningKey, (services, _) => services.GetRequiredService<SigningCredentials>())
    .AddKeyedSingleton<SigningCredentials>(ServiceKeys.SessionTicketSigningKey, (services, _) => services.GetRequiredService<SigningCredentials>())
    .AddSingleton<JwtSecurityTokenHandler>(services =>
    {
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        jwtSecurityTokenHandler.InboundClaimTypeMap.Clear();
        jwtSecurityTokenHandler.OutboundClaimTypeMap.Clear();

        return jwtSecurityTokenHandler;
    })
    .AddSingleton<TokenValidationParameters>(services => new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "hintkeep",
        ValidateAudience = true,
        ValidAudience = "graphql",
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = services.GetRequiredService<SecurityKey>(),
        ClockSkew = TimeSpan.Zero
    })
    .AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigure>();

// Hashing
builder
    .Services
    .AddTransient<HashAlgorithm>(services =>
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var hashKey = configuration.GetValue<string>("HINTKEEP_HASH_KEY");

        if (string.IsNullOrWhiteSpace(hashKey))
        {
            logger.LogCritical("HINTKEEP_HASH_KEY has not been configured.");
            throw new InvalidOperationException("Expected HINTKEEP_HASH_KEY to be configured.");
        }

        return new HMACSHA256(Encoding.UTF8.GetBytes(hashKey));
    })
    .AddKeyedScoped<HashAlgorithm>(ServiceKeys.UsernameHashAlgorithm, (services, _) => services.GetRequiredService<HashAlgorithm>())
    .AddKeyedScoped<HashAlgorithm>(ServiceKeys.PasswordHashAlgorithm, (services, _) => services.GetRequiredService<HashAlgorithm>())
    .AddKeyedScoped<HashAlgorithm>(ServiceKeys.EmailAddressHashAlgorithm, (services, _) => services.GetRequiredService<HashAlgorithm>());

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
    .AddSingleton<AuthorizationSettings>(services =>
    {
        var authorizationSettings = new AuthorizationSettings();
        authorizationSettings.AddPolicy(
            "authenticatedUser",
            policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(HintKeepClaims.UserId)
                .AddRequirement(new GuidRequirement(HintKeepClaims.UserId, "Expected '" + HintKeepClaims.UserId + "' to be a GUID."))
                .RequireClaim(HintKeepClaims.TokenId)
                .AddRequirement(new GuidRequirement(HintKeepClaims.TokenId, "Expected '" + HintKeepClaims.TokenId + "' to be a GUID."))
                .AddRequirement(new GuidRequirement(HintKeepClaims.SessionId, "Expected '" + HintKeepClaims.SessionId + "' to be a GUID."))
        );

        return authorizationSettings;
    })
    .AddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>()
    .AddSingleton<IValidationRule, AuthorizationValidationRule>()
    .AddSingleton<ISchema>(services =>
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var defaultAuthorization = new AuthorizeAttribute("authenticatedUser");
        var mutaitonGraphObject = new ObjectGraphType
        {
            Name = "HintKeepMutations",
            Description = "Contains all of HintKeep mutations for performing create, update or delete operations."
        };
        foreach (var mutationField in services.GetKeyedServices<FieldType>(mutatoinFieldTypeKey))
        {
            var mutationFieldBuilder = mutaitonGraphObject.AddField(mutationField);
            var mutationFieldAttribute = mutationField.GetType().GetCustomAttribute<MutationFieldAttribute>();
            var authorizeAttributes = mutationField
                .GetType()
                .GetCustomAttributes<AuthorizeAttribute>()
                .OrderBy(authorizeAttribute => authorizeAttribute.Priority)
                .AsEnumerable();
            if (mutationFieldAttribute?.AllowAnonymous is false)
                authorizeAttributes = authorizeAttributes.DefaultIfEmpty(defaultAuthorization);

            foreach (var authorizeAttribute in authorizeAttributes)
            {
                if (!string.IsNullOrWhiteSpace(authorizeAttribute.Roles))
                {
                    logger.LogCritical("Authorization roles are not supported at this time.");
                    throw new InvalidOperationException("Authorization roles are not supported at this time.");
                }
                if (!string.IsNullOrWhiteSpace(authorizeAttribute.Policy))
                    mutationFieldBuilder.AuthorizeWithPolicy(authorizeAttribute.Policy);
            }
        }

        return new Schema
        {
            Description = "HintKeep GraphQL API",
            NameConverter = CamelCaseNameConverter.Instance,
            Query = services.GetRequiredService<QueryGraphDefinition>(),
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
        var tableStorageUri = configuration.GetSection("AzureWebJobsStorage").GetValue<string>("tableServiceUri");

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
            throw new InvalidOperationException("Expected either AzureWebJobsStorage (connection string) or AzureWebJobsStorage__tableServiceUri (managed identity) to be configured.");
        }
    });

// Logging
builder
    .UseMiddleware(
        async (context, next) =>
        {
            var httpContext = context.GetHttpContext();
            var correlationId = context.InstanceServices.GetRequiredKeyedService<Guid>(ServiceKeys.CorrelationId);
            var logger = context.InstanceServices.GetRequiredService<ILogger<Program>>();

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

internal class JwtBearerPostConfigure(ILogger<Program> logger, TokenValidationParameters tokenValidationParameters) : IPostConfigureOptions<JwtBearerOptions>
{
    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        options.TokenValidationParameters = tokenValidationParameters;
        options.Events.OnAuthenticationFailed += context =>
        {
            var userId = context.Principal?.FindFirstValue(HintKeepClaims.UserId);
            var tokenId = context.Principal?.FindFirstValue(HintKeepClaims.TokenId);
            logger.LogWarning("Failed to authenticate JWT '{tokenId}' having user ID '{userId}' with error '{error}'.", tokenId, userId, context.Exception.Message);

            return Task.CompletedTask;
        };
        options.Events.OnTokenValidated += context =>
        {
            var userId = context.Principal?.FindFirstValue(HintKeepClaims.UserId);
            var tokenId = context.Principal?.FindFirstValue(HintKeepClaims.TokenId);
            logger.LogInformation("Authenticated JWT '{tokenId}' having user ID '{userId}'.", tokenId, userId);

            return Task.CompletedTask;
        };
    }
}

class GuidRequirement(string claim, string errorMessage) : IAuthorizationRequirement
{
    public Task Authorize(AuthorizationContext context)
    {
        var claimValue = context.User?.FindFirstValue(claim);

        if (claimValue is not null && (string.IsNullOrWhiteSpace(claimValue) || !Guid.TryParseExact(claimValue, HintKeepClaims.GuidFormatString, out var _)))
            context.ReportError(errorMessage);

        return Task.CompletedTask;
    }
}