using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HintKeep.GraphQL.Definitions.UserAccounts;

public static class HintKeepClaims
{
    public const string UserId = JwtRegisteredClaimNames.Sub;
    public const string TokenId = JwtRegisteredClaimNames.Jti;

    internal const string GuidFormatString = "D";

    public static Guid GetUserId(this ClaimsPrincipal? claimsPrincipal)
    {
        var userId = claimsPrincipal!.FindFirstValue(UserId)!;
        return Guid.ParseExact(userId, GuidFormatString);
    }
}