using System.Security.Claims;
using GraphQL.Authorization;
using HintKeep.GraphQL.Definitions.Users;

namespace HintKeep.GraphQL.AuthorizationPolicies;

internal class GuidRequirement(string claim, string errorMessage) : IAuthorizationRequirement
{
    public Task Authorize(AuthorizationContext context)
    {
        var claimValue = context.User?.FindFirstValue(claim);

        if (claimValue is not null && (string.IsNullOrWhiteSpace(claimValue) || !Guid.TryParseExact(claimValue, HintKeepClaims.GuidFormatString, out var _)))
            context.ReportError(errorMessage);

        return Task.CompletedTask;
    }
}