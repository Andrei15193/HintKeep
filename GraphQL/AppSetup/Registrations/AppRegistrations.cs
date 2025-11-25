using HintKeep.GraphQL.AuthorizationPolicies;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.AppSetup.Registrations;

internal static class AppRegistrations
{
    public static FunctionsApplicationBuilder AddHintKeepServices(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.Services.AddHintKeepServices();
        return (FunctionsApplicationBuilder)builder;
    }

    public static IServiceCollection AddHintKeepServices(this IServiceCollection services)
        => services
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
}