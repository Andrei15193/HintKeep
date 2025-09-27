using HintKeep.GraphQL.Definitions.Users;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.AuthorizationPolicies;

public static class AuthenticatedUserPolicy
{
    public static IServiceCollection AddAuthenticatedUserPolicy(this IServiceCollection services, out string name)
        => services.AddAuthorizationPolicy(
            name = "authenticatedUser",
            policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(HintKeepClaims.UserId)
                .AddRequirement(new GuidRequirement(HintKeepClaims.UserId, "Expected '" + HintKeepClaims.UserId + "' to be a GUID."))
                .RequireClaim(HintKeepClaims.TokenId)
                .AddRequirement(new GuidRequirement(HintKeepClaims.TokenId, "Expected '" + HintKeepClaims.TokenId + "' to be a GUID."))
                .AddRequirement(new GuidRequirement(HintKeepClaims.SessionId, "Expected '" + HintKeepClaims.SessionId + "' to be a GUID."))
        );
}