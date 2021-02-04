using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Accounts
{
    public class AccountUpdate
    {
        [RequiredMediumText]
        public string Name { get; set; }

        [RequiredMediumText]
        public string Hint { get; set; }

        public bool IsPinned { get; set; }
    }
}