using GraphQL;
using HintKeep.GraphQL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HintKeep.GraphQL.AuthorizationPolicies;
using HintKeep.GraphQL.AppSetup.Middlewares;
using HintKeep.GraphQL.AppSetup.Registrations;

var builder = FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFunctionsWebApplication();

// Middleware
builder
    .UseMiddleware<LoggingMiddleware>()
    .UseMiddleware<AuthenticationMiddleware>()
    .UseMiddleware<GraphiQLMiddlewareAdapter>()
    .Services

// Monitoring
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddCorrelationId()

// Hashing
    .AddHmacSha256HashAlgorithm(ServiceKeys.UsernameHashAlgorithm, "HINTKEEP_HASH_KEY")
    .AddHmacSha256HashAlgorithm(ServiceKeys.PasswordHashAlgorithm, "HINTKEEP_HASH_KEY")
    .AddHmacSha256HashAlgorithm(ServiceKeys.EmailAddressHashAlgorithm, "HINTKEEP_HASH_KEY")

// Authentication (JSON Web Tokens)
    .AddJsonWebTokens(ServiceKeys.SessionTicket, "HINTKEEP_SIGNING_KEY")
    .AddJsonWebTokens(ServiceKeys.SessionToken, "HINTKEEP_SIGNING_KEY")

// Serialization
    .AddJsonSerialization()

// GraphQL
    .AddAuthenticatedUserPolicy(out var defaultAuthorizationPolicyName)
    .AddGraphQL(defaultAuthorizationPolicyName)

// Request handlers
    .AddRequestHandlers()

// Azure Storage
    .AddAzureStorage();

builder
    .Build()
    .Run();