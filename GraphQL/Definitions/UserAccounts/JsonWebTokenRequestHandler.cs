
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.GraphQL.Definitions.UserAccounts;

public record JsonWebTokenRequest(Guid UserId) : IRequest<string>;

public class JsonWebTokenRequestHandler(SigningCredentials signingCredentials, TokenValidationParameters tokenValidationParameters, JwtSecurityTokenHandler jsonWebTokenHandler) : IRequestHandler<JsonWebTokenRequest, string>
{
    public ValueTask<string> ExecuteAsync(JsonWebTokenRequest request, CancellationToken cancellationToken)
    {
        var token = jsonWebTokenHandler.WriteToken(new JwtSecurityToken(
            issuer: tokenValidationParameters.ValidIssuer,
            audience: tokenValidationParameters.ValidAudience,
            claims: [
                new Claim(HintKeepClaims.UserId, request.UserId.ToString("D")),
                new Claim(HintKeepClaims.TokenId, Guid.NewGuid().ToString("D"))
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials
        ));

        return ValueTask.FromResult(token);
    }
}