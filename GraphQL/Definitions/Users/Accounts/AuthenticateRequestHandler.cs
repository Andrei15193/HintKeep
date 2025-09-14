using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Azure.Data.Tables;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;
using HintKeep.GraphQL.Definitions.Users.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.Definitions.Users.Accounts;

public record AuthenticateRequest(
    [property: Required(ErrorMessage = "A username is required.")]
    string Username,

    [property: Required(ErrorMessage = "A password is required.")]
    string Password
) : IRequest<AuthenticateResult>;

public record AuthenticateResult(
    Guid UserId,
    Guid SessionId,
    string Username,
    string SessionToken,
    DateTime SessionTokenExpiration,
    string SessionTicket,
    DateTime SessionTicketExpiration
);

public class AuthenticateRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    [FromKeyedServices(ServiceKeys.UsernameHashAlgorithm)] HashAlgorithm usernameHashAlgorithm,
    [FromKeyedServices(ServiceKeys.PasswordHashAlgorithm)] HashAlgorithm passwordHashAlgorithm,

    IRequestHandler<CreateSessionTokenRequest, CreateSessionTokenResult> sessionTokenRequestHandler,
    IRequestHandler<CreateSessionTicketRequest, CreateSessionTicketResult> ensureSessionTicketRequestHandler
) : IRequestHandler<AuthenticateRequest, AuthenticateResult>
{
    public async ValueTask<AuthenticateResult> ExecuteAsync(AuthenticateRequest request, CancellationToken cancellationToken)
    {
        var usernameHash = Convert.ToHexString(usernameHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Username.ToLowerInvariant())));
        var passwordHash = Convert.ToHexString(passwordHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

        var userPasswordHashEntity = await hintKeepTableStorage
            .Users
            .GetEntityIfExistsAsync<TableEntity>(UserPasswordHashEntity.GetEntityKey(usernameHash, passwordHash), cancellationToken)
            .ToUserPasswordHashEntity()
            ?? throw new ValidationException(
                new ValidationResult("Invalid credentials.", [nameof(AuthenticateRequest.Username), nameof(AuthenticateRequest.Password)]),
                null,
                null
            );

        var userEntity = await hintKeepTableStorage
            .Users
            .GetEntityIfExistsAsync<TableEntity>(UserIdUniqueEntity.GetEntityKey(userPasswordHashEntity.UserId), cancellationToken)
            .ToUserIdUniqueEntity()
            ?? throw new ValidationException(
                new ValidationResult("Invalid credentials.", [nameof(AuthenticateRequest.Username), nameof(AuthenticateRequest.Password)]),
                null,
                null
            );

        var (sessionToken, sessionId, sessionTokenExpiration) = await sessionTokenRequestHandler.ExecuteAsync(
            new CreateSessionTokenRequest(UserId: userEntity.UserId).EnsureValid(),
            cancellationToken
        );
        var (sessionTicket, sessionTicketExpiration) = await ensureSessionTicketRequestHandler.ExecuteAsync(
            new CreateSessionTicketRequest(UserId: userEntity.UserId).EnsureValid(),
            cancellationToken
        );

        return new AuthenticateResult(
            UserId: userEntity.UserId,
            SessionId: sessionId,
            Username: userEntity.Username,
            SessionToken: sessionToken,
            SessionTokenExpiration: sessionTokenExpiration,
            SessionTicket: sessionTicket,
            SessionTicketExpiration: sessionTicketExpiration
        );
    }
}