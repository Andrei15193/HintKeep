using System.IdentityModel.Tokens.Jwt;
using Azure;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

public record EndSessionRequest(
    Guid UserId,
    Guid SessionId,
    string? SessionTicket
) :
    IRequest<EndSessionResult>;

public record EndSessionResult(bool Success);

public class EndSessionRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    JwtSecurityTokenHandler jsonWebTokenHandler
) :
    IRequestHandler<EndSessionRequest, EndSessionResult>
{
    public async ValueTask<EndSessionResult> ExecuteAsync(EndSessionRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.SessionTicket))
        {
            var sessionTicketSecurityToken = jsonWebTokenHandler.ReadJwtToken(request.SessionTicket);
            var ticketId = sessionTicketSecurityToken.GetTokenId();

            await hintKeepTableStorage.UserSessions.DeleteEntityAsync(
                UserSessionTicketEntity.GetEntityKey(request.UserId, ticketId),
                ETag.All,
                cancellationToken
            );
        }

        await hintKeepTableStorage.UserSessions.DeleteEntityAsync(
                UserSessionEntity.GetEntityKey(request.UserId, request.SessionId),
                ETag.All,
                cancellationToken: cancellationToken
            );

        return new EndSessionResult(
            Success: true
        );
    }
}