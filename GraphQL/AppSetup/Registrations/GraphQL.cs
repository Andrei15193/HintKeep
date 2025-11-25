using System.Reflection;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Conversion;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using GraphQL.Validation;
using HintKeep.GraphQL.AuthorizationPolicies;
using HintKeep.GraphQL.Definitions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HintKeep.GraphQL.AppSetup.Registrations;

internal static class GraphQL
{
    public static IServiceCollection AddGraphQL(this IServiceCollection services, string defaultAuthorizationPolicyName)
    {
        var mutatoinFieldTypeKey = new object();

        foreach (var type in typeof(Program).Assembly.DefinedTypes)
        {
            if (typeof(IGraphType).IsAssignableFrom(type))
                services.AddSingleton(type, type);
            if (typeof(FieldType).IsAssignableFrom(type) && type.GetCustomAttribute<MutationFieldAttribute>() is not null)
                services.AddKeyedSingleton(typeof(FieldType), mutatoinFieldTypeKey, type);
        }

        services
            .AddSingleton<IGraphQLTextSerializer, GraphQLSerializer>()
            .AddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>()
            .AddSingleton<IValidationRule, AuthorizationValidationRule>()
            .AddSingleton<AuthorizationSettings>(services =>
            {
                var authorizationPolicies = services.GetServices<INamedAuthorizationPolicy>();

                var authorizationSettings = new AuthorizationSettings();
                foreach (var authorizationPolicy in authorizationPolicies)
                    authorizationSettings.AddPolicy(authorizationPolicy.Name, authorizationPolicy);

                return authorizationSettings;
            })
            .AddSingleton<ISchema>(services =>
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                var defaultAuthorization = new AuthorizeAttribute(defaultAuthorizationPolicyName);
                var mutationGraphObject = new ObjectGraphType
                {
                    Name = "HintKeepMutations",
                    Description = "Contains all of HintKeep mutations for performing create, update or delete operations."
                };
                foreach (var mutationField in services.GetKeyedServices<FieldType>(mutatoinFieldTypeKey))
                {
                    var mutationFieldBuilder = mutationGraphObject.AddField(mutationField);
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
                    Mutation = mutationGraphObject
                };
            });

        return services;
    }
}