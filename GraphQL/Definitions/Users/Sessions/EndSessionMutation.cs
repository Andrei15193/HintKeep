using GraphQL;
using GraphQL.Types;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

[MutationField]
public class EndSessionMutation : RequestFieldType<EndSessionRequest, EndSessionResult>
{
    public EndSessionMutation()
    {
        Name = "EndSession";
        Arguments = [];
        Type = typeof(AutoRegisteringObjectGraphType<EndSessionResult>);
    }

    protected override EndSessionRequest GetInput(IResolveFieldContext context)
    {
        var userId = context.User.GetUserId();
        var sessionId = context.User.GetSessionId();
        var sessionTicket = context
            .GetHttpRequestData()
            .Cookies
            .SingleOrDefault(cookie => cookie.Name == HintKeepHttp.SessionTicketCookieName)
            ?.Value;

        return new EndSessionRequest(
            UserId: userId,
            SessionId: sessionId,
            SessionTicket: sessionTicket
        );
    }

    protected override ValueTask<EndSessionResult?> ResolveAsync(IResolveFieldContext context)
    {
        context.SetHttpResponseCookie(
            HintKeepHttp.SessionTicketCookieName,
            string.Empty,
            DateTime.UtcNow
        );
        context.SetHttpResponseCookie(
            HintKeepHttp.SessionTokenCookieName(context.User.GetSessionId()),
            string.Empty,
            DateTime.UtcNow
        );
        context.SetDevHttpResponseCookie(
            HintKeepHttp.DevSessionTokenCookieName,
            string.Empty,
            DateTime.UtcNow
        );

        return base.ResolveAsync(context);
    }
}