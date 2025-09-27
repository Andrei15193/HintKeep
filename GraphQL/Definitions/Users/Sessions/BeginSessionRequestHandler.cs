using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Azure;
using Azure.Data.Tables;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;
using HintKeep.GraphQL.Definitions.Users.Accounts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

public record BeginSessionRequest(
    [property: Required(ErrorMessage = "Session ticket is missing.")]
    string? SessionTicket
) : IRequest<AuthenticationResult>;

public class BeginSessionRequestHandler(
    IHostEnvironment environment,
    ILogger<BeginSessionRequestHandler> logger,
    HintKeepTableStorage hintKeepTableStorage,

    TokenValidationParameters tokenValidationParameters,
    JwtSecurityTokenHandler jsonWebTokenHandler,

    IRequestHandler<CreateSessionTokenRequest, CreateSessionTokenResult> sessionTokenRequestHandler,
    IRequestHandler<CreateSessionTicketRequest, CreateSessionTicketResult> sessionTicketRequestHandler
) : IRequestHandler<BeginSessionRequest, AuthenticationResult>
{
    public async ValueTask<AuthenticationResult> ExecuteAsync(BeginSessionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            jsonWebTokenHandler.ValidateToken(request.SessionTicket, tokenValidationParameters, out var _);
        }
        catch (SecurityTokenException securityTokenException)
        {
            logger.LogWarning(securityTokenException, "Invalid Session Ticket JSON Web Token '{jwt}'.", request.SessionTicket);

            var validationErrorMessage = environment.IsDevelopment()
                ? $"Invalid session ticket. {securityTokenException}"
                : "Invalid session ticket.";
            throw new ValidationException(new ValidationResult(validationErrorMessage, [nameof(BeginSessionRequest.SessionTicket)]), null, null);
        }

        var sessionTicketSecurityToken = jsonWebTokenHandler.ReadJwtToken(request.SessionTicket);
        var userId = sessionTicketSecurityToken.GetUserId();
        var ticketId = sessionTicketSecurityToken.GetTokenId();

        var userSessionTicketEntity = await hintKeepTableStorage
            .UserSessions
            .GetEntityIfExistsAsync<TableEntity>(UserSessionTicketEntity.GetEntityKey(userId, ticketId), cancellationToken)
            .ToUserSessionTicketEntity()
            ?? throw new ValidationException(
                new ValidationResult("Session ticket expired.", [nameof(BeginSessionRequest.SessionTicket)]),
                null,
                null
            );
        await hintKeepTableStorage
            .UserSessions
            .DeleteEntityAsync(userSessionTicketEntity.ToTableEntity(), ETag.All, cancellationToken);

        if (userSessionTicketEntity.TicketExpiration <= DateTime.UtcNow)
            throw new ValidationException(
                new ValidationResult("Session ticket expired.", [nameof(BeginSessionRequest.SessionTicket)]),
                null,
                null
            );

        var userEntity = await hintKeepTableStorage
            .Users
            .GetEntityIfExistsAsync<TableEntity>(UserIdUniqueEntity.GetEntityKey(userId), cancellationToken)
            .ToUserIdUniqueEntity()
            ?? throw new ValidationException(
                new ValidationResult("Session ticket expired.", [nameof(BeginSessionRequest.SessionTicket)]),
                null,
                null
            );

        var sessionTokenResult = await sessionTokenRequestHandler.ExecuteAsync(
            new CreateSessionTokenRequest(userId).EnsureValid(),
            cancellationToken
        );
        var sessionTicketResult = await sessionTicketRequestHandler.ExecuteAsync(
            new CreateSessionTicketRequest(userId).EnsureValid(),
            cancellationToken
        );

        return new AuthenticationResult(
            UserId: userId,
            SessionId: sessionTokenResult.SessionId,
            Username: userEntity.Username,
            SessionToken: sessionTokenResult.SessionToken,
            SessionTokenExpiration: sessionTokenResult.SessionTokenExpiration,
            SessionTicket: sessionTicketResult.Ticket,
            SessionTicketExpiration: sessionTicketResult.TicketExpiration
        );
    }
}