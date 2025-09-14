using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HintKeep.GraphQL.Definitions.Users;

public static class HintKeepClaims
{
    public const string UserId = JwtRegisteredClaimNames.Sub;
    public const string SessionId = JwtRegisteredClaimNames.Sid;
    public const string TokenId = JwtRegisteredClaimNames.Jti;

    internal const string GuidFormatString = "D";

    public static Guid GetUserId(this ClaimsPrincipal? claimsPrincipal)
        => GetGuidToken(claimsPrincipal, UserId);

    public static Guid GetUserId(this JwtSecurityToken? claims)
        => GetGuidToken(claims, UserId);

    public static Guid GetSessionId(this ClaimsPrincipal? claimsPrincipal)
        => GetGuidToken(claimsPrincipal, SessionId);

    public static Guid GetSessionId(this JwtSecurityToken? claims)
        => GetGuidToken(claims, SessionId);

    public static Guid GetTokenId(this ClaimsPrincipal? claimsPrincipal)
        => GetGuidToken(claimsPrincipal, TokenId);

    public static Guid GetTokenId(this JwtSecurityToken? claims)
        => GetGuidToken(claims, TokenId);

    private static Guid GetGuidToken(ClaimsPrincipal? claimsPrincipal, string claimType)
    {
        var claimValue = claimsPrincipal!.FindFirstValue(claimType)!;
        return Guid.ParseExact(claimValue, GuidFormatString);
    }

    private static Guid GetGuidToken(JwtSecurityToken?  securityToken, string claimType)
    {
        var claimValue = securityToken!.Claims!.FirstOrDefault(claim => claim.Type == claimType)!.Value;
        return Guid.ParseExact(claimValue, GuidFormatString);
    }
}