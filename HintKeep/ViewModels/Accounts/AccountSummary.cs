namespace HintKeep.ViewModels.Accounts
{
    public record AccountSummary(
        string Id,
        string Name,
        string Hint,
        bool IsPinned
    );
}