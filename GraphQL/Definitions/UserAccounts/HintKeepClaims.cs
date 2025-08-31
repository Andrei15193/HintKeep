using System.IdentityModel.Tokens.Jwt;

namespace HintKeep.GraphQL.Definitions.UserAccounts;

public static class HintKeepClaims
{
    public const string UserId = JwtRegisteredClaimNames.Sub;
    public const string TokenId = JwtRegisteredClaimNames.Jti;
}