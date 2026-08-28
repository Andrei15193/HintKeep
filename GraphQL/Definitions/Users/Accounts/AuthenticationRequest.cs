using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Azure.Data.Tables;
using GraphQL;
using GraphQL.Types;
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

[MutationField(AllowAnonymous = true)]
public class AuthenticateMutation : RequestFieldType<AuthenticationRequest, AuthenticationResult>
{
    public AuthenticateMutation()
    {
        Name = "Authenticate";

        Arguments =
        [
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(AuthenticationRequest.Username)
            },
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(AuthenticationRequest.Password)
            }
        ];
        Type = typeof(AuthenticationResultGraphType);
    }

    protected override AuthenticationRequest GetInput(IResolveFieldContext context)
        => new(
            Username: context.GetArgument<string>(nameof(AuthenticationRequest.Username)),
            Password: context.GetArgument<string>(nameof(AuthenticationRequest.Password))
        );
}

public class AuthenticationResultGraphType : ObjectGraphType<AuthenticationResult>
{
    public AuthenticationResultGraphType()
    {
        Field<NonNullGraphType<GuidGraphType>>(nameof(AuthenticationResult.UserId))
            .Resolve(context => _EnsureAuthenticationCookies(context).Source.UserId);
        Field<NonNullGraphType<GuidGraphType>>(nameof(AuthenticationResult.SessionId))
            .Resolve(context => _EnsureAuthenticationCookies(context).Source.SessionId);
        Field<NonNullGraphType<StringGraphType>>(nameof(AuthenticationResult.SessionRenewTicket))
            .Resolve(context => _EnsureAuthenticationCookies(context).Source.SessionRenewTicket);
        Field<NonNullGraphType<StringGraphType>>(nameof(AuthenticationResult.Username))
            .Resolve(context => _EnsureAuthenticationCookies(context).Source.Username);
    }

    private IResolveFieldContext<AuthenticationResult> _EnsureAuthenticationCookies(IResolveFieldContext<AuthenticationResult> context)
    {
        const string authenticationCookiesMarker = "hint-keep-auth-cookies-handled";

        var result = context.Source;
        if (result is not null && !context.UserContext.ContainsKey(authenticationCookiesMarker) && context.Errors.Count == 0)
        {
            context.SetHttpCookie(
                HintKeepHttp.SessionTicketCookieName,
                result.SessionTicket,
                result.SessionTicketExpiration
            );
            context.SetHttpCookie(
                HintKeepHttp.SessionTokenCookieName(result.SessionId),
                result.SessionToken,
                result.SessionTokenExpiration
            );
            context.SetDevHttpCookie(
                HintKeepHttp.DevSessionTokenCookieName,
                result.SessionToken,
                result.SessionTokenExpiration
            );

            context.UserContext.Add(authenticationCookiesMarker, null);
        }

        return context;
    }
}

public class AuthenticationRequestHandler(
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
        var prefixedUsernameHash = UserUniqueEntity.UsernamePrefix + Convert.ToHexString(usernameHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Username.ToLowerInvariant())));
        var passwordHash = Convert.ToHexString(passwordHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

        var userPasswordHashEntity = await hintKeepTableStorage
            .Users
            .GetEntityIfExistsAsync<TableEntity>(UserPasswordHashEntity.GetEntityKey(prefixedUsernameHash, passwordHash), cancellationToken)
            .ToUserPasswordHashEntity()
            ?? throw new ValidationException(
                new ValidationResult("Wrong credentials. Please try again or follow the password recovery steps", [nameof(AuthenticationRequest.Username), nameof(AuthenticationRequest.Password)]),
                null,
                null
            );

        var userEntity = await hintKeepTableStorage
            .Users
            .GetEntityIfExistsAsync<TableEntity>(UserIdUniqueEntity.GetEntityKey(userPasswordHashEntity.UserId), cancellationToken)
            .ToUserIdUniqueEntity()
            ?? throw new ValidationException(
                new ValidationResult("Wrong credentials. Please try again or follow the password recovery steps", [nameof(AuthenticationRequest.Username), nameof(AuthenticationRequest.Password)]),
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