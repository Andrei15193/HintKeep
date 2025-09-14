using GraphQL;
using GraphQL.Types;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

[MutationField(AllowAnonymous = true)]
public class BeginSessionMutation : RequestFieldType<BeginSessionRequest, BeginSessionResult>
{
    public BeginSessionMutation()
    {
        Name = "beginSession";

        Arguments = [];
        Type = typeof(BeginSessionMutationResultGraphType);
    }

    protected override BeginSessionRequest GetInput(IResolveFieldContext context)
        => new(
            SessionTicket: context
                .GetHttpRequestData()
                .Cookies
                .SingleOrDefault(cookie => cookie.Name == HintKeepHttp.SessionTicketCookieName)
                ?.Value
        );

    protected override async ValueTask<BeginSessionResult?> ResolveAsync(IResolveFieldContext context)
    {
        var result = await base.ResolveAsync(context);
        if (context.Errors.Count == 0 && result is not null)
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
        }

        return result;
    }

    private class BeginSessionMutationResultGraphType : ObjectGraphType<BeginSessionResult>
    {
        public BeginSessionMutationResultGraphType()
        {
            Field(x => x.UserId);
            Field(x => x.SessionId);
            Field(x => x.Username);
        }
    }
}