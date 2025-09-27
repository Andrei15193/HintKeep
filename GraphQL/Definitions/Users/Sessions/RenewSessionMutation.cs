using GraphQL;
using GraphQL.Types;
using HintKeep.GraphQL.Definitions.Users.Accounts;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

[MutationField]
public class RenewSessionMutation : RequestFieldType<RenewSessionRequest, RenewSessionResult>
{
    public RenewSessionMutation()
    {
        Name = "RenewSession";

        Arguments = [
            new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = nameof(RenewSessionRequest.SessionRenewToken)
            }
        ];
        Type = typeof(RenewSessionResultGraphType);
    }

    protected override RenewSessionRequest GetInput(IResolveFieldContext context)
        => new(
            UserId: context.User.GetUserId(),
            SessionId: context.User.GetSessionId(),
            SessionRenewToken: context.GetArgument<string>(nameof(RenewSessionRequest.SessionRenewToken))
        );
}

public class RenewSessionResultGraphType : ObjectGraphType<RenewSessionResult>
{
    public RenewSessionResultGraphType()
    {
        Field<NonNullGraphType<StringGraphType>>(nameof(RenewSessionResult.SessionRenewToken))
            .Resolve(context => _EnsureAuthenticationCookies(context).Source.SessionRenewToken);
    }

    private IResolveFieldContext<RenewSessionResult> _EnsureAuthenticationCookies(IResolveFieldContext<RenewSessionResult> context)
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