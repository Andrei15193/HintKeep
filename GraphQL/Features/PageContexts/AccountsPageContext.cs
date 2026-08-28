using HintKeep.GraphQL.Features.Contexts;

namespace HintKeep.GraphQL.Features.PageContexts;

public record AccountsPageContext() : PageContext(
    new Dictionary<string, object?>
    {
    }
)
{
    public override string Name => "accounts";

    public override IReadOnlyDictionary<string, PageContextAction> LinkActions
        => _linkActions;
    private static readonly IReadOnlyDictionary<string, PageContextAction> _linkActions = new Dictionary<string, PageContextAction>
    {
    };

    public override IReadOnlyDictionary<string, PageContextAction> ButtonActions
        => _buttonActions;
    private static readonly IReadOnlyDictionary<string, PageContextAction> _buttonActions = new Dictionary<string, PageContextAction>
    {
    };
}