
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

public record GenerateSessionTicketRequest(
    Guid UserId,
    Guid TicketId
) :
    IRequest<GenerateSessionTicketResult>;

public record GenerateSessionTicketResult(
    string Ticket,
    DateTime TicketExpiration
);

public class GenerateSessionTicketRequestHandler(
    [FromKeyedServices(ServiceKeys.SessionTicketSigningKey)] SigningCredentials signingCredentials,
    TokenValidationParameters tokenValidationParameters,
    JwtSecurityTokenHandler jsonWebTokenHandler
) :
    IRequestHandler<GenerateSessionTicketRequest, GenerateSessionTicketResult>
{
    public ValueTask<GenerateSessionTicketResult> ExecuteAsync(GenerateSessionTicketRequest request, CancellationToken cancellationToken)
    {
        var ticketId = request.TicketId;
        var sessionTicketExpiration = DateTime.UtcNow.AddDays(1.5);

        var sessionTicket = jsonWebTokenHandler.WriteToken(new JwtSecurityToken(
            issuer: tokenValidationParameters.ValidIssuer,
            audience: tokenValidationParameters.ValidAudience,
            claims: [
                new Claim(HintKeepClaims.UserId, request.UserId.ToString(HintKeepClaims.GuidFormatString)),
                new Claim(HintKeepClaims.TokenId, ticketId.ToString(HintKeepClaims.GuidFormatString))
            ],
            expires: sessionTicketExpiration,
            signingCredentials: signingCredentials
        ));

        return ValueTask.FromResult(new GenerateSessionTicketResult(
            Ticket: sessionTicket,
            TicketExpiration: sessionTicketExpiration
        ));
    }
}