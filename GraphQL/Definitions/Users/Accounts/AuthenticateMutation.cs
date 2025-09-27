using GraphQL;
using GraphQL.Types;

namespace HintKeep.GraphQL.Definitions.Users.Accounts;

[MutationField(AllowAnonymous = true)]
public class AuthenticateMutation : RequestFieldType<AuthenticationRequest, AuthenticationResult>
{
    public AuthenticateMutation()
    {
        Name = "authenticate";

        Arguments =
        [
            new QueryArgument<StringGraphType>
            {
                Name = nameof(AuthenticationRequest.Username)
            },
            new QueryArgument<StringGraphType>
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
        Field<NonNullGraphType<StringGraphType>>(nameof(AuthenticationResult.Username))
            .Resolve(context => _EnsureAuthenticationCookies(context).Source.Username);
    }

    private IResolveFieldContext<AuthenticationResult> _EnsureAuthenticationCookies(IResolveFieldContext<AuthenticationResult> context)
    {
        const string authenticationCookiesMarker = "hint-keep-auth-cookies-handled";

        var result = context.Source;
        if (result is not null && !context.UserContext.ContainsKey(authenticationCookiesMarker) && context.Errors.Count == 0)
        {
            context.SetHttpResponseCookie(
                HintKeepHttp.SessionTicketCookieName,
                result.SessionTicket,
                result.SessionTicketExpiration
            );
            context.SetHttpResponseCookie(
                HintKeepHttp.SessionTokenCookieName(result.SessionId),
                result.SessionToken,
                result.SessionTokenExpiration
            );
            context.SetDevHttpResponseCookie(
                HintKeepHttp.DevSessionTokenCookieName,
                result.SessionToken,
                result.SessionTokenExpiration
            );

            context.UserContext.Add(authenticationCookiesMarker, null);
        }

        return context;
    }
}