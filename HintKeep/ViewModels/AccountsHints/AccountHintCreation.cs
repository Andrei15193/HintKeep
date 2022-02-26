using System;
using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.AccountsHints
{
    public record AccountHintCreation(
        [RequiredMediumText] string Hint,
        DateTime? DateAdded
    );
}