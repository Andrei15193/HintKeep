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

    public override IReadOnlyDictionary<string, string> FormFieldMappings { get; } = new Dictionary<string, string>
    {
        { "confirm password", "confirmPassword" }
    };

    public override IReadOnlyDictionary<string, PageContextAction> LinkActions { get; } = new Dictionary<string, PageContextAction>
    {
        {
            "cancel", (applicationContext, cancellationToken) => {
                applicationContext.PageContext = new LogInPageContext();
                return Task.CompletedTask;
            }
        }
    };

    public override IReadOnlyDictionary<string, PageContextAction> ButtonActions { get; } = new Dictionary<string, PageContextAction>
    {
        {
            "sign up", async (applicationContext, cancellationToken) =>
            {
                const string RegisterUserAccountMutation = @"
                    mutation($username: String!, $password: String!, $hint: String!, $email: String!) {
                        registerUserAccount(username: $username, password: $password, hint: $hint, email: $email) {
                            userId
                            username
                            sessionId
                            sessionRenewTicket
                        }
                    }
                ";

                if (!Equals(applicationContext.PageContext.FormData["password"], applicationContext.PageContext.FormData["confirm password"]))
                    applicationContext.PageContext = applicationContext.PageContext with
                    {
                        FormErrorData = new Dictionary<string, string>
                        {
                            { "password", "Passwords do not match"}
                        }
                    };
                else {
                    var dataResult = await applicationContext.CallGraphFormAsync(RegisterUserAccountMutation, cancellationToken);

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
}