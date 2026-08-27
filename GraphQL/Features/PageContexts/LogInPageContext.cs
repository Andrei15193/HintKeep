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

    public override IReadOnlyDictionary<string, Func<ApplicationContext, Task>> LinkActions
        => _linkActions;
    private static readonly IReadOnlyDictionary<string, Func<ApplicationContext, Task>> _linkActions = new Dictionary<string, Func<ApplicationContext, Task>>
    {
        {
            "sign up", applicationContext => {
                applicationContext.PageContext = new SignUpPageContext();
                return Task.CompletedTask;
            }
        }
    };

    public override IReadOnlyDictionary<string, Func<ApplicationContext, Task>> ButtonActions
        => _buttonActions;
    private static readonly IReadOnlyDictionary<string, Func<ApplicationContext, Task>> _buttonActions = new Dictionary<string, Func<ApplicationContext, Task>>
    {
    };
}
