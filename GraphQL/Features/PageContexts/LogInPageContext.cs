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
    };
}
