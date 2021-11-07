using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public class UserRegistrationConfirmation
    {
        [RequiredMediumText]
        public string Token { get; set; }
    }
}