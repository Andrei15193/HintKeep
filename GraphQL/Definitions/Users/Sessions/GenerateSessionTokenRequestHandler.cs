
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

public record GenerateSessionTokenRequest(
    Guid UserId,
    Guid SessionId
) :
    IRequest<GenerateSessionTokenResult>;

public record GenerateSessionTokenResult(
    string SessionToken,
    string SessionRenewTicket,
    DateTime SessionTokenExpiration
);

public class GenerateSessionTokenRequestHandler(
    [FromKeyedServices(ServiceKeys.SessionToken)] SigningCredentials signingCredentials,
    [FromKeyedServices(ServiceKeys.SessionToken)] TokenValidationParameters tokenValidationParameters,
    JwtSecurityTokenHandler jsonWebTokenHandler
) :
    IRequestHandler<GenerateSessionTokenRequest, GenerateSessionTokenResult>
{
    public ValueTask<GenerateSessionTokenResult> ExecuteAsync(GenerateSessionTokenRequest request, CancellationToken cancellationToken)
    {
        var sessionRenewTicket = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
        var sessionTokenExpiration = DateTime.UtcNow.AddHours(1);

        var sessionToken = jsonWebTokenHandler.WriteToken(new JwtSecurityToken(
            issuer: tokenValidationParameters.ValidIssuer,
            audience: tokenValidationParameters.ValidAudience,
            claims: [
                new Claim(HintKeepClaims.UserId, request.UserId.ToString(HintKeepClaims.GuidFormatString)),
                new Claim(HintKeepClaims.SessionId, request.SessionId.ToString(HintKeepClaims.GuidFormatString)),
                new Claim(HintKeepClaims.TokenId, Guid.NewGuid().ToString(HintKeepClaims.GuidFormatString))
            ],
            expires: sessionTokenExpiration,
            signingCredentials: signingCredentials
        ));

        return ValueTask.FromResult(new GenerateSessionTokenResult(
            SessionToken: sessionToken,
            SessionRenewTicket: sessionRenewTicket,
            SessionTokenExpiration: sessionTokenExpiration
        ));
    }
}