using HintKeep.GraphQL.Features.Contexts;

namespace HintKeep.GraphQL.Features.PageContexts;

public record AccountsPageContext() : PageContext(
    new Dictionary<string, object?>
    {
    }
)
{
    public override string Name => "accounts";

    public override IReadOnlyDictionary<string, Func<ApplicationContext, Task>> LinkActions
        => _linkActions;
    private static readonly IReadOnlyDictionary<string, Func<ApplicationContext, Task>> _linkActions = new Dictionary<string, Func<ApplicationContext, Task>>
    {
    };

    public override IReadOnlyDictionary<string, Func<ApplicationContext, Task>> ButtonActions
        => _buttonActions;
    private static readonly IReadOnlyDictionary<string, Func<ApplicationContext, Task>> _buttonActions = new Dictionary<string, Func<ApplicationContext, Task>>
    {
    };
}