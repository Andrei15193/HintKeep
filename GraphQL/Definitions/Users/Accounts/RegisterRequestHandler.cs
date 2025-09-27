using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.Data.Tables;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;
using HintKeep.GraphQL.Definitions.Users.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.Definitions.Users.Accounts;

public record RegisterRequest(
    [property: Required(ErrorMessage = "A username is required.")]
    string Username,

    [property: Required(ErrorMessage = "A password is required.")]
    string Password,

    [property: Required(ErrorMessage = "An email address is required.")]
    string EmailAddress
) :
    IRequest<AuthenticationResult>;

public class RegisterRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    [FromKeyedServices(ServiceKeys.UsernameHashAlgorithm)] HashAlgorithm usernameHashAlgorithm,
    [FromKeyedServices(ServiceKeys.PasswordHashAlgorithm)] HashAlgorithm passwordHashAlgorithm,
    [FromKeyedServices(ServiceKeys.EmailAddressHashAlgorithm)] HashAlgorithm emailHashAlgorithm,

    IRequestHandler<CreateSessionTicketRequest, CreateSessionTicketResult> sessionTicketRequestHandler,
    IRequestHandler<CreateSessionTokenRequest, CreateSessionTokenResult> sessionTokenRequestHandler
) :
    IRequestHandler<RegisterRequest, AuthenticationResult>
{
    public async ValueTask<AuthenticationResult> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var usernameHash = Convert.ToHexString(usernameHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Username.ToLowerInvariant())));
        var passwordHash = Convert.ToHexString(passwordHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));
        var emailAddressHash = Convert.ToHexString(emailHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.EmailAddress)));

        if (
            await hintKeepTableStorage
                .Users
                .GetEntityIfExistsAsync<TableEntity>(UserUniqueEntity.GetEntityKey(usernameHash), cancellationToken) is not null
        )
            throw new ValidationException(new ValidationResult("Usernames must be unique.", [nameof(RegisterRequest.Username)]), null, null);

        var userId = await _ReserveUserIdAsync(usernameHash, request.Username, cancellationToken);

        try
        {
            await hintKeepTableStorage.Users.SubmitTransactionAsync(
                [
                    new TableTransactionAction(
                        TableTransactionActionType.Add,
                        new UserUniqueEntity(
                            UsernameHash: usernameHash,
                            UserId: userId
                        ).ToTableEntity()
                    ),
                    new TableTransactionAction(
                        TableTransactionActionType.Add,
                        new UserPasswordHashEntity(usernameHash, passwordHash, userId).ToTableEntity()
                    ),
                    new TableTransactionAction(
                        TableTransactionActionType.Add,
                        new UserEmailAddressHashEntity(usernameHash, emailAddressHash).ToTableEntity()
                    )
                ],
                cancellationToken
            );
        }
        catch (TableTransactionFailedException tableTransactionFailedException) when (tableTransactionFailedException.Status == (int)HttpStatusCode.Conflict)
        {
            throw new ValidationException(new ValidationResult("Usernames must be unique.", [nameof(RegisterRequest.Username)]), null, null);
        }

        var sessionTicketResult = await sessionTicketRequestHandler.ExecuteAsync(
            new CreateSessionTicketRequest(
                UserId: userId
            ).EnsureValid(),
            cancellationToken
        );
        var sessionTokenResult = await sessionTokenRequestHandler.ExecuteAsync(
            new CreateSessionTokenRequest(
                UserId: userId,
                SessionTicketId: sessionTicketResult.TicketId
            ).EnsureValid(),
            cancellationToken
        );

        return new AuthenticationResult(
            UserId: userId,
            SessionId: sessionTokenResult.SessionId,
            Username: request.Username,

            SessionToken: sessionTokenResult.SessionToken,
            SessionRenewToken: sessionTokenResult.SessionRenewToken,
            SessionTokenExpiration: sessionTokenResult.SessionTokenExpiration,

            SessionTicket: sessionTicketResult.Ticket,
            SessionTicketExpiration: sessionTicketResult.TicketExpiration
        );
    }

    private async ValueTask<Guid> _ReserveUserIdAsync(string usernameHash, string username, CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid();
        var isIdReserved = false;

        do
            try
            {
                await hintKeepTableStorage.Users.AddEntityAsync(new UserIdUniqueEntity(userId, usernameHash, username).ToTableEntity(), cancellationToken);
                isIdReserved = true;
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                userId = Guid.NewGuid();
            }
        while (!isIdReserved);

        return userId;
    }
}