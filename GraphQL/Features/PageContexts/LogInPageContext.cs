using HintKeep.GraphQL.Features.Contexts;

namespace HintKeep.GraphQL.Features.PageContexts;

public record LogInPageContext() : PageContext(
    new Dictionary<string, object?>
    {
        { "username", string.Empty },
        { "password", string.Empty }
    }
)
{
    public override string Name => "login";

    public override IReadOnlyDictionary<string, PageContextAction> LinkActions
        => _linkActions;
    private static readonly IReadOnlyDictionary<string, PageContextAction> _linkActions = new Dictionary<string, PageContextAction>
    {
        {
            "sign up", (applicationContext, cancellationToken) => {
                applicationContext.PageContext = new SignUpPageContext();
                return Task.CompletedTask;
            }
        }
    };

    public override IReadOnlyDictionary<string, PageContextAction> ButtonActions
        => _buttonActions;
    private static readonly IReadOnlyDictionary<string, PageContextAction> _buttonActions = new Dictionary<string, PageContextAction>
    {
        {
            "login", async (applicationContext, cancellationToken) =>
            {
                const string AuthenticateMutation = @"
                    mutation($username: String!, $password: String!) {
                        authenticate(username: $username, password: $password) {
                            userId
                            username
                            sessionId
                            sessionRenewTicket
                        }
                    }
                ";

                var dataResult = await applicationContext.CallGraphFormAsync(AuthenticateMutation, cancellationToken);

                if (
                    dataResult is not null
                    && dataResult.AsObject().TryGetPropertyValue("authenticate", out var mutationResult)
                    && mutationResult is not null
                )
                {
                    var user = new UserContext(
                        UserId: mutationResult["userId"]!.GetValue<string>(),
                        Username: mutationResult["username"]!.GetValue<string>(),
                        SessionId: mutationResult["sessionId"]!.GetValue<string>(),
                        SessionRenewTicket: mutationResult["sessionRenewTicket"]!.GetValue<string>()
                    );
                    Assert.Multiple(
                        () => Assert.NotNull(user.UserId),
                        () => Assert.NotNull(user.Username),
                        () => Assert.NotNull(user.SessionId),
                        () => Assert.NotNull(user.SessionRenewTicket)
                    );

                    applicationContext.CurrentUser = user;
                    applicationContext.PageContext = new AccountsPageContext();
                }
            }
        },
        { "login with localDB", (application, cancellationToken) => Task.CompletedTask }
    };
}
