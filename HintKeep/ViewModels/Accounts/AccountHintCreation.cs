using System;
using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Accounts
{
    public class AccountHintCreation
    {
        [RequiredMediumText]
        public string Hint { get; set; }

        public DateTime? DateAdded { get; set; }
    }
}