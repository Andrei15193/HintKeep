using GraphQL;
using HintKeep.GraphQL.Definitions.Users.Accounts;

namespace HintKeep.GraphQL.Definitions.Users.Sessions;

[MutationField(AllowAnonymous = true)]
public class BeginSessionMutation : RequestFieldType<BeginSessionRequest, AuthenticationResult>
{
    public BeginSessionMutation()
    {
        Name = "BeginSession";

        Arguments = [];
        Type = typeof(AuthenticationResultGraphType);
    }

    protected override BeginSessionRequest GetInput(IResolveFieldContext context)
        => new(
            SessionTicket: context
                .GetHttpRequestData()
                .Cookies
                .SingleOrDefault(cookie => cookie.Name == HintKeepHttp.SessionTicketCookieName)
                ?.Value
        );
}