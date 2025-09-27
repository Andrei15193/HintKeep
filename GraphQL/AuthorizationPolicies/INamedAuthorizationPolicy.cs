using GraphQL.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.AuthorizationPolicies;

public interface INamedAuthorizationPolicy : IAuthorizationPolicy
{
    string Name { get; }
}

public static class NamedAuthorizationPolicyHelper
{
    public static IServiceCollection AddAuthorizationPolicy(this IServiceCollection services, string name, Action<AuthorizationPolicyBuilder> configure)
    {
        var builder = new AuthorizationPolicyBuilder();
        configure(builder);

        return services
            .AddSingleton<INamedAuthorizationPolicy>(new NamedAuthorizationPolicy(name, builder.Build()));
    }

    public static IServiceCollection AddAuthorizationPolicy(this IServiceCollection services, string name, IAuthorizationPolicy authorizationPolicy)
        => services
            .AddSingleton<INamedAuthorizationPolicy>(new NamedAuthorizationPolicy(name, authorizationPolicy));

    private class NamedAuthorizationPolicy(string name, IAuthorizationPolicy authorizationPolicy) : INamedAuthorizationPolicy
    {
        public string Name
            => name;

        public IEnumerable<IAuthorizationRequirement> Requirements
            => authorizationPolicy.Requirements;
    }
}