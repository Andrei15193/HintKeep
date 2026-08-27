using HintKeep.GraphQL.Features.Contexts;

namespace HintKeep.GraphQL.Features.PageContexts;

public record SignUpPageContext() : PageContext(
    new Dictionary<string, object?>
    {
        { "username", string.Empty },
        { "password", string.Empty },
        { "confirm password", string.Empty },
        { "hint", string.Empty },
        { "email", string.Empty }
    }
)
{
    public override string Name => "sign up";

    public override IReadOnlyDictionary<string, Func<ApplicationContext, Task>> LinkActions
        => _linkActions;
    private static readonly IReadOnlyDictionary<string, Func<ApplicationContext, Task>> _linkActions = new Dictionary<string, Func<ApplicationContext, Task>>
    {
        {
            "cancel", applicationContext => {
                applicationContext.PageContext = new LogInPageContext();
                return Task.CompletedTask;
            }
        }
    };

    public override IReadOnlyDictionary<string, Func<ApplicationContext, Task>> ButtonActions
        => _buttonActions;
    private static readonly IReadOnlyDictionary<string, Func<ApplicationContext, Task>> _buttonActions = new Dictionary<string, Func<ApplicationContext, Task>>
    {
        {
            "sign up", async applicationContext =>
            {
                if (!Equals(applicationContext.PageContext.FormData["password"], applicationContext.PageContext.FormData["confirm password"]))
                    applicationContext.PageContext = applicationContext.PageContext with
                    {
                        FormErrorData = new Dictionary<string, string>
                        {
                            { "password", "Passwords do not match"}
                        }
                    };
                else {
                    var dataResult = await applicationContext.CallGraphAsync(RegisterUserAccountMutation, applicationContext.PageContext.FormData);

                    if (
                        dataResult is not null
                        && dataResult.AsObject().TryGetPropertyValue("registerUserAccount", out var mutationResult)
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
                    else if (Equals(applicationContext.PageContext.FormData["confirm password"], ""))
                        applicationContext.PageContext = applicationContext.PageContext with
                        {
                            FormErrorData = new Dictionary<string, string>(applicationContext.PageContext.FormErrorData)
                            {
                                { "confirm password", "A matching password is required"}
                            }
                        };
                    else if (!Equals(applicationContext.PageContext.FormData["password"], applicationContext.PageContext.FormData["confirm password"]))
                        applicationContext.PageContext = applicationContext.PageContext with
                        {
                            FormErrorData = new Dictionary<string, string>(applicationContext.PageContext.FormErrorData)
                            {
                                { "confirm password", "A matching password is required"}
                            }
                        };
                }
            }
        }
    };

    private const string RegisterUserAccountMutation = @"
        mutation($username: String!, $password: String!, $hint: String!, $email: String!) {
            registerUserAccount(username: $username, password: $password, hint: $hint, email: $email) {
                userId
                sessionId
                sessionRenewTicket
                username
            }
        }";
}