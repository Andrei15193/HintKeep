namespace HintKeep.ViewModels.Accounts
{
    public record AccountDetails(
        string Id,
        string Name,
        string Hint,
        string Notes,
        bool IsPinned
    );
}