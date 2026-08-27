using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.Data.Tables;
using GraphQL;
using GraphQL.Types;
using HintKeep.GraphQL.Data;
using HintKeep.GraphQL.Data.Users;
using HintKeep.GraphQL.Definitions.Users.Sessions;
using HintKeep.GraphQL.Definitions.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.GraphQL.Definitions.Users.Accounts;

public record RegisterUserAccountRequest(
    [property: Required(ErrorMessage = "A username is required"), MaxLength(250, ErrorMessage = "The username can be at most 250 characters")]
    string Username,

    [property: Required(ErrorMessage = "A password is required"), MaxLength(250, ErrorMessage = "The password can be at most 250 characters"), PasswordStrength(ErrorMessage = "The password must be strong, at least 8 characters long containing both lowercase and uppercase letters alongside at least one numeric and special character")]
    string Password,

    [property: Required(ErrorMessage = "A hint is required"), MaxLength(250, ErrorMessage = "The hint can be at most 250 characters")]
    string Hint,

    [property: Required(ErrorMessage = "An email address is required"), EmailAddress(ErrorMessage = "A valid email address is required"), MaxLength(250, ErrorMessage = "The email address can be at most 250 characters")]
    string Email
) :
    IRequest<AuthenticationResult>;

[MutationField(AllowAnonymous = true)]
public class RegisterUserAccountRequestMutation : RequestFieldType<RegisterUserAccountRequest, AuthenticationResult>
{
    public RegisterUserAccountRequestMutation()
    {
        Name = "RegisterUserAccount";

        Arguments =
        [
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(RegisterUserAccountRequest.Username)
            },
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(RegisterUserAccountRequest.Password)
            },
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(RegisterUserAccountRequest.Hint)
            },
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(RegisterUserAccountRequest.Email)
            }
        ];
        Type = typeof(AuthenticationResultGraphType);
    }

    protected override RegisterUserAccountRequest GetInput(IResolveFieldContext context)
        => new(
            Username: context.GetArgument<string>(nameof(RegisterUserAccountRequest.Username)),
            Password: context.GetArgument<string>(nameof(RegisterUserAccountRequest.Password)),
            Hint: context.GetArgument<string>(nameof(RegisterUserAccountRequest.Hint)),
            Email: context.GetArgument<string>(nameof(RegisterUserAccountRequest.Email))
        );
}

public class RegisterUserAccountRequestHandler(
    HintKeepTableStorage hintKeepTableStorage,

    [FromKeyedServices(ServiceKeys.UsernameHashAlgorithm)] HashAlgorithm usernameHashAlgorithm,
    [FromKeyedServices(ServiceKeys.PasswordHashAlgorithm)] HashAlgorithm passwordHashAlgorithm,
    [FromKeyedServices(ServiceKeys.EmailAddressHashAlgorithm)] HashAlgorithm emailHashAlgorithm,

    IRequestHandler<CreateSessionTicketRequest, CreateSessionTicketResult> sessionTicketRequestHandler,
    IRequestHandler<CreateSessionTokenRequest, CreateSessionTokenResult> sessionTokenRequestHandler
) :
    IRequestHandler<RegisterUserAccountRequest, AuthenticationResult>
{
    public async ValueTask<AuthenticationResult> ExecuteAsync(RegisterUserAccountRequest request, CancellationToken cancellationToken)
    {
        var prefixedUsernameHash = UserUniqueEntity.UsernamePrefix + Convert.ToHexString(usernameHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Username.ToLowerInvariant())));
        var prefixedEmailHash = UserUniqueEntity.EmailPrefix + Convert.ToHexString(emailHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Email.ToLowerInvariant())));
        var passwordHash = Convert.ToHexString(passwordHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

        if (
            await hintKeepTableStorage
                .Users
                .GetEntityIfExistsAsync<TableEntity>(UserUniqueEntity.GetEntityKey(prefixedUsernameHash), cancellationToken)
                .ToUserUniqueEntity() is not null
        )
            throw new ValidationException(new ValidationResult("The username is unavailable", [nameof(RegisterUserAccountRequest.Username)]), null, null);
        if (
            await hintKeepTableStorage
                .Users
                .GetEntityIfExistsAsync<TableEntity>(UserUniqueEntity.GetEntityKey(prefixedEmailHash), cancellationToken)
                .ToUserUniqueEntity() is not null
        )
            throw new ValidationException(new ValidationResult("The email address is unavailable", [nameof(RegisterUserAccountRequest.Email)]), null, null);

        var userId = await _ReserveUserIdAsync(prefixedUsernameHash, request.Username, cancellationToken);

        try
        {
            await hintKeepTableStorage.Users.AddEntityAsync(
                new UserUniqueEntity(
                    PropertyHash: prefixedEmailHash,
                    UserId: userId
                ).ToTableEntity(),
                cancellationToken
            );
        }
        catch (TableTransactionFailedException tableTransactionFailedException) when (tableTransactionFailedException.Status == (int)HttpStatusCode.Conflict)
        {
            throw new ValidationException(new ValidationResult("The email address is unavailable", [nameof(RegisterUserAccountRequest.Email)]), null, null);
        }
        try
        {
            await hintKeepTableStorage.Users.SubmitTransactionAsync(
                [
                    new TableTransactionAction(
                        TableTransactionActionType.Add,
                        new UserUniqueEntity(
                            PropertyHash: prefixedUsernameHash,
                            UserId: userId
                        ).ToTableEntity()
                    ),
                    new TableTransactionAction(
                        TableTransactionActionType.Add,
                        new UserPasswordHashEntity(prefixedUsernameHash, passwordHash, userId, request.Hint).ToTableEntity()
                    ),
                    new TableTransactionAction(
                        TableTransactionActionType.Add,
                        new UserEmailAddressHashEntity(prefixedUsernameHash, prefixedEmailHash).ToTableEntity()
                    )
                ]
            );
        }
        catch (TableTransactionFailedException tableTransactionFailedException) when (tableTransactionFailedException.Status == (int)HttpStatusCode.Conflict)
        {
            throw new ValidationException(new ValidationResult("The username is unavailable", [nameof(RegisterUserAccountRequest.Username)]), null, null);
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
            SessionRenewTicket: sessionTokenResult.SessionRenewTicket,
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