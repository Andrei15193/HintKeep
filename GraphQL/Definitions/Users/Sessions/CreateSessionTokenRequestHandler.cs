using System.Net;
using Azure;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

public record CreateSessionTokenRequest(
    Guid UserId,
    Guid SessionTicketId
) :
    IRequest<CreateSessionTokenResult>;

public record CreateSessionTokenResult(
    string SessionToken,
    Guid SessionId,
    string SessionRenewTicket,
    DateTime SessionTokenExpiration
);

public class CreateSessionTokenRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    IRequestHandler<GenerateSessionTokenRequest, GenerateSessionTokenResult> generateSessionTokenRequestHandler
) :
    IRequestHandler<CreateSessionTokenRequest, CreateSessionTokenResult>
{
    public async ValueTask<CreateSessionTokenResult> ExecuteAsync(CreateSessionTokenRequest request, CancellationToken cancellationToken)
    {
        var hasSessionId = false;
        var sessionId = Guid.NewGuid();
        GenerateSessionTokenResult generateSessionTokenResult;

        do
        {
            generateSessionTokenResult = await generateSessionTokenRequestHandler.ExecuteAsync(
                new GenerateSessionTokenRequest(
                    UserId: request.UserId,
                    SessionId: sessionId
                ).EnsureValid(),
                cancellationToken
            );
            try
            {
                await hintKeepTableStorage.UserSessions.AddEntityAsync(
                    new UserSessionEntity(
                        UserId: request.UserId,
                        SessionId: sessionId,
                        SessionTicketId: request.SessionTicketId,
                        RenewTicket: generateSessionTokenResult.SessionRenewTicket,
                        TokenExpiration: generateSessionTokenResult.SessionTokenExpiration
                    ).ToTableEntity(),
                    cancellationToken
                );
                hasSessionId = true;
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                sessionId = Guid.NewGuid();
            }
        } while (!hasSessionId);


        return new CreateSessionTokenResult(
            SessionToken: generateSessionTokenResult.SessionToken,
            SessionId: sessionId,
            SessionRenewTicket: generateSessionTokenResult.SessionRenewTicket,
            SessionTokenExpiration: generateSessionTokenResult.SessionTokenExpiration
        );
    }
}