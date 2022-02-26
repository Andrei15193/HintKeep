using System;

namespace HintKeep.ViewModels.AccountsHints
{
    public record AccountHintDetails(
        string Id,
        string Hint,
        DateTime? DateAdded
    );
}