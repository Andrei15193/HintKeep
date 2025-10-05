using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

public record RenewSessionRequest(
    Guid UserId,
    Guid SessionId,

    [property: Required(ErrorMessage = "Session ticket is missing.")]
    string SessionRenewTicket
) :
    IRequest<RenewSessionResult>;

public record RenewSessionResult(
    string SessionToken,
    Guid SessionId,
    string SessionRenewTicket,
    DateTime SessionTokenExpiration,

    string SessionTicket,
    DateTime SessionTicketExpiration
);

public class RenewSessionRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    IRequestHandler<CreateSessionTicketRequest, CreateSessionTicketResult> createSessionTicketRequestHandler,
    IRequestHandler<GenerateSessionTokenRequest, GenerateSessionTokenResult> generateSessionTokenRequestHandler
) :
    IRequestHandler<RenewSessionRequest, RenewSessionResult>
{
    public async ValueTask<RenewSessionResult> ExecuteAsync(RenewSessionRequest request, CancellationToken cancellationToken)
    {
        var userSession = await hintKeepTableStorage
            .UserSessions
            .GetEntityIfExistsAsync<TableEntity>(UserSessionEntity.GetEntityKey(request.UserId, request.SessionId), cancellationToken)
            .ToUserSessionEntity()
            ?? throw new ValidationException(
                new ValidationResult("Session expired.", [nameof(RenewSessionRequest.SessionId)]),
                null,
                null
            );
        if (
            userSession.TokenExpiration <= DateTime.UtcNow
            || userSession.RenewTicket != request.SessionRenewTicket
        )
            throw new ValidationException(
                new ValidationResult("Session expired.", [nameof(RenewSessionRequest.SessionId)]),
                null,
                null
            );

        var userSessionTicket = await hintKeepTableStorage
            .UserSessions
            .GetEntityIfExistsAsync<TableEntity>(UserSessionTicketEntity.GetEntityKey(request.UserId, userSession.SessionTicketId), cancellationToken)
            .ToUserSessionTicketEntity();
        if (userSessionTicket is not null)
            await hintKeepTableStorage
                .UserSessions
                .DeleteEntityAsync(userSessionTicket.Value.ToTableEntity(), ETag.All, cancellationToken);

        var createSessionTicketResult = await createSessionTicketRequestHandler.ExecuteAsync(
            new CreateSessionTicketRequest(
                UserId: request.UserId
            ).EnsureValid(),
            cancellationToken
        );
        var generateSessionTokenResult = await generateSessionTokenRequestHandler.ExecuteAsync(
            new GenerateSessionTokenRequest(
                UserId: request.UserId,
                SessionId: request.SessionId
            ).EnsureValid(),
            cancellationToken
        );

        await hintKeepTableStorage.UserSessions.UpdateEntityAsync(
            (
                userSession with
                {
                    SessionTicketId = createSessionTicketResult.TicketId,
                    RenewTicket = generateSessionTokenResult.SessionRenewTicket,
                    TokenExpiration = generateSessionTokenResult.SessionTokenExpiration
                }
            ).ToTableEntity(),
            userSession.ETag,
            TableUpdateMode.Replace,
            cancellationToken
        );

        return new RenewSessionResult(
            SessionToken: generateSessionTokenResult.SessionToken,
            SessionId: request.SessionId,
            SessionRenewTicket: generateSessionTokenResult.SessionRenewTicket,
            SessionTokenExpiration: generateSessionTokenResult.SessionTokenExpiration,

            SessionTicket: createSessionTicketResult.Ticket,
            SessionTicketExpiration: createSessionTicketResult.TicketExpiration
        );
    }
}