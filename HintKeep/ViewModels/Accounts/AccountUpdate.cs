using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Accounts
{
    public record AccountUpdate(
        [RequiredMediumText] string Name,
        [RequiredMediumText]        string Hint,
        [LongText] string Notes,
        bool IsPinned
    );
}