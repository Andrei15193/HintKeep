
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Azure;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

public record CreateSessionTokenRequest(Guid UserId) : IRequest<CreateSessionTokenResult>;

public record CreateSessionTokenResult(string SessionToken, Guid SessionId, DateTime SessionExpiration);

public class CreateSessionTokenRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    SigningCredentials signingCredentials,
    TokenValidationParameters tokenValidationParameters,
    JwtSecurityTokenHandler jsonWebTokenHandler
) : IRequestHandler<CreateSessionTokenRequest, CreateSessionTokenResult>
{
    public async ValueTask<CreateSessionTokenResult> ExecuteAsync(CreateSessionTokenRequest request, CancellationToken cancellationToken)
    {
        var hasSessionId = false;
        var sessionId = Guid.NewGuid();
        var sessionExpiration = DateTime.UtcNow.AddHours(1);

        do
            try
            {
                await hintKeepTableStorage.Users.AddEntityAsync(
                    new UserSessionEntity(
                        UserId: request.UserId,
                        SessionId: sessionId
                    ).ToTableEntity(),
                    cancellationToken
                );
                hasSessionId = true;
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                sessionId = Guid.NewGuid();
            }
        while (!hasSessionId);

        var token = jsonWebTokenHandler.WriteToken(new JwtSecurityToken(
            issuer: tokenValidationParameters.ValidIssuer,
            audience: tokenValidationParameters.ValidAudience,
            claims: [
                new Claim(HintKeepClaims.UserId, request.UserId.ToString(HintKeepClaims.GuidFormatString)),
                new Claim(HintKeepClaims.SessionId, sessionId.ToString(HintKeepClaims.GuidFormatString)),
                new Claim(HintKeepClaims.TokenId, Guid.NewGuid().ToString(HintKeepClaims.GuidFormatString))
            ],
            expires: sessionExpiration,
            signingCredentials: signingCredentials
        ));

        return new CreateSessionTokenResult(
            SessionToken: token,
            SessionId: sessionId,
            SessionExpiration: sessionExpiration
        );
    }
}