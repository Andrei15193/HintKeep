using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Azure.Data.Tables;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;
using HintKeep.GraphQL.Definitions.Users.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.Definitions.Users.Accounts;

public record AuthenticationRequest(
    [property: Required(ErrorMessage = "A username is required.")]
    string Username,

    [property: Required(ErrorMessage = "A password is required.")]
    string Password
) :
    IRequest<AuthenticationResult>;

public record AuthenticationResult(
    Guid UserId,
    Guid SessionId,
    string Username,

    string SessionToken,
    string SessionRenewTicket,
    DateTime SessionTokenExpiration,

    string SessionTicket,
    DateTime SessionTicketExpiration
);

public class AuthenticateRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    [FromKeyedServices(ServiceKeys.UsernameHashAlgorithm)] HashAlgorithm usernameHashAlgorithm,
    [FromKeyedServices(ServiceKeys.PasswordHashAlgorithm)] HashAlgorithm passwordHashAlgorithm,

    IRequestHandler<CreateSessionTicketRequest, CreateSessionTicketResult> sessionTicketRequestHandler,
    IRequestHandler<CreateSessionTokenRequest, CreateSessionTokenResult> sessionTokenRequestHandler
) :
    IRequestHandler<AuthenticationRequest, AuthenticationResult>
{
    public async ValueTask<AuthenticationResult> ExecuteAsync(AuthenticationRequest request, CancellationToken cancellationToken)
    {
        var usernameHash = Convert.ToHexString(usernameHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Username.ToLowerInvariant())));
        var passwordHash = Convert.ToHexString(passwordHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

        var userPasswordHashEntity = await hintKeepTableStorage
            .Users
            .GetEntityIfExistsAsync<TableEntity>(UserPasswordHashEntity.GetEntityKey(usernameHash, passwordHash), cancellationToken)
            .ToUserPasswordHashEntity()
            ?? throw new ValidationException(
                new ValidationResult("Invalid credentials.", [nameof(AuthenticationRequest.Username), nameof(AuthenticationRequest.Password)]),
                null,
                null
            );

        var userEntity = await hintKeepTableStorage
            .Users
            .GetEntityIfExistsAsync<TableEntity>(UserIdUniqueEntity.GetEntityKey(userPasswordHashEntity.UserId), cancellationToken)
            .ToUserIdUniqueEntity()
            ?? throw new ValidationException(
                new ValidationResult("Invalid credentials.", [nameof(AuthenticationRequest.Username), nameof(AuthenticationRequest.Password)]),
                null,
                null
            );

        var sessionTicketResult = await sessionTicketRequestHandler.ExecuteAsync(
            new CreateSessionTicketRequest(
                UserId: userEntity.UserId
            ).EnsureValid(),
            cancellationToken
        );
        var sessionTokenResult = await sessionTokenRequestHandler.ExecuteAsync(
            new CreateSessionTokenRequest(
                UserId: userEntity.UserId,
                SessionTicketId: sessionTicketResult.TicketId
            ).EnsureValid(),
            cancellationToken
        );

        return new AuthenticationResult(
            UserId: userEntity.UserId,
            SessionId: sessionTokenResult.SessionId,
            Username: userEntity.Username,

            SessionToken: sessionTokenResult.SessionToken,
            SessionRenewTicket: sessionTokenResult.SessionRenewTicket,
            SessionTokenExpiration: sessionTokenResult.SessionTokenExpiration,

            SessionTicket: sessionTicketResult.Ticket,
            SessionTicketExpiration: sessionTicketResult.TicketExpiration
        );
    }
}