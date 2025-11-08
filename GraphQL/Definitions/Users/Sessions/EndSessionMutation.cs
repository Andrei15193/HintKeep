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
        var sessionTicket = context.GetHttpCookie(HintKeepHttp.SessionTicketCookieName);

        return new EndSessionRequest(
            UserId: userId,
            SessionId: sessionId,
            SessionTicket: sessionTicket
        );
    }

    protected override ValueTask<EndSessionResult?> ResolveAsync(IResolveFieldContext context)
    {
        context.SetHttpCookie(
            HintKeepHttp.SessionTicketCookieName,
            string.Empty,
            DateTime.UtcNow
        );
        context.SetHttpCookie(
            HintKeepHttp.SessionTokenCookieName(context.User.GetSessionId()),
            string.Empty,
            DateTime.UtcNow
        );
        context.SetDevHttpCookie(
            HintKeepHttp.DevSessionTokenCookieName,
            string.Empty,
            DateTime.UtcNow
        );

        return base.ResolveAsync(context);
    }
}