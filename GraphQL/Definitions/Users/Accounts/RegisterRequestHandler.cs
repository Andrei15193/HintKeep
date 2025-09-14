using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.Data.Tables;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.Definitions.Users.Accounts;

public record RegisterRequest(
    [property: Required(ErrorMessage = "A username is required.")]
    string Username,

    [property: Required(ErrorMessage = "A password is required.")]
    string Password,

    [property: Required(ErrorMessage = "An email address is required.")]
    string EmailAddress
) : IRequest<RegisterResult>;

public record RegisterResult(
    Guid UserId,
    string Username
);

public class RegisterRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    [FromKeyedServices(ServiceKeys.UsernameHashAlgorithm)] HashAlgorithm usernameHashAlgorithm,
    [FromKeyedServices(ServiceKeys.PasswordHashAlgorithm)] HashAlgorithm passwordHashAlgorithm,
    [FromKeyedServices(ServiceKeys.EmailAddressHashAlgorithm)] HashAlgorithm emailHashAlgorithm
) : IRequestHandler<RegisterRequest, RegisterResult>
{
    public async ValueTask<RegisterResult> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var usernameHash = Convert.ToHexString(usernameHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Username.ToLowerInvariant())));
        var passwordHash = Convert.ToHexString(passwordHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));
        var emailAddressHash = Convert.ToHexString(emailHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.EmailAddress)));

        // quick username hash check (avoid user ID reservation flooding through DDoS)

        var userId = await _ReserveUserIdAsync(usernameHash, request.Username, cancellationToken);

        try
        {
            await hintKeepTableStorage.Users.SubmitTransactionAsync(
                [
                    new TableTransactionAction(
                        TableTransactionActionType.Add,
                        new UserUniqueEntity(usernameHash).ToTableEntity()
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

        return new RegisterResult(
            UserId: userId,
            Username: request.Username
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