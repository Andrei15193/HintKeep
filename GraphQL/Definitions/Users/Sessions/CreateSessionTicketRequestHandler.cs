
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Azure;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

public record CreateSessionTicketRequest(Guid UserId) : IRequest<CreateSessionTicketResult>;

public record CreateSessionTicketResult(string Ticket, DateTime TicketExpiration);

public class CreateSessionTicketRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    [FromKeyedServices(ServiceKeys.SessionTicketSigningKey)] SigningCredentials signingCredentials,
    TokenValidationParameters tokenValidationParameters,
    JwtSecurityTokenHandler jsonWebTokenHandler
) : IRequestHandler<CreateSessionTicketRequest, CreateSessionTicketResult>
{
    public async ValueTask<CreateSessionTicketResult> ExecuteAsync(CreateSessionTicketRequest request, CancellationToken cancellationToken)
    {
        var hasTicketId = false;
        var ticketId = Guid.NewGuid();
        var ticketExpiration = DateTime.UtcNow.AddDays(1.5);
        do
            try
            {
                await hintKeepTableStorage.Users.AddEntityAsync(
                    new UserSessionTicketEntity(
                        UserId: request.UserId,
                        TicketId: ticketId,
                        TicketExpiration: ticketExpiration,
                        ETag: ETag.All
                    ).ToTableEntity(),
                    cancellationToken
                );
                hasTicketId = true;
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                ticketId = Guid.NewGuid();
            }
        while (hasTicketId is false);

        var sessionTicket = jsonWebTokenHandler.WriteToken(new JwtSecurityToken(
            issuer: tokenValidationParameters.ValidIssuer,
            audience: tokenValidationParameters.ValidAudience,
            claims: [
                new Claim(HintKeepClaims.UserId, request.UserId.ToString(HintKeepClaims.GuidFormatString)),
                new Claim(HintKeepClaims.TokenId, ticketId.ToString(HintKeepClaims.GuidFormatString))
            ],
            expires: ticketExpiration,
            signingCredentials: signingCredentials
        ));

        return new(
            Ticket: sessionTicket,
            TicketExpiration: ticketExpiration
        );
    }
}