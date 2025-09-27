using System.Net;
using Azure;
using Azure.Data.Tables;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

public record CreateSessionTicketRequest(
    Guid UserId
) :
    IRequest<CreateSessionTicketResult>;

public record CreateSessionTicketResult(
    string Ticket,
    Guid TicketId,
    DateTime TicketExpiration
);

public class CreateSessionTicketRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    IRequestHandler<GenerateSessionTicketRequest, GenerateSessionTicketResult> generateSessionTicketRequestHandler
) :
    IRequestHandler<CreateSessionTicketRequest, CreateSessionTicketResult>
{
    public async ValueTask<CreateSessionTicketResult> ExecuteAsync(CreateSessionTicketRequest request, CancellationToken cancellationToken)
    {
        var hasTicketId = false;

        var ticketId = Guid.NewGuid();
        GenerateSessionTicketResult generateSessionTicketResult;

        do
        {
            generateSessionTicketResult = await generateSessionTicketRequestHandler.ExecuteAsync(
                new GenerateSessionTicketRequest(UserId: request.UserId, TicketId: ticketId).EnsureValid(),
                cancellationToken
            );
            try
            {
                await hintKeepTableStorage.UserSessions.AddEntityAsync(
                    new UserSessionTicketEntity(
                        UserId: request.UserId,
                        TicketId: ticketId,
                        TicketExpiration: generateSessionTicketResult.TicketExpiration
                    ).ToTableEntity(),
                    cancellationToken
                );
                hasTicketId = true;
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                ticketId = Guid.NewGuid();
            }
        } while (hasTicketId is false);

        return new(
            Ticket: generateSessionTicketResult.Ticket,
            TicketId: ticketId,
            TicketExpiration: generateSessionTicketResult.TicketExpiration
        );
    }
}