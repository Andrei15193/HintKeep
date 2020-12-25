using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public class UserConfirmation
    {
        [Email]
        public string Email { get; set; }

        [ConfirmationToken]
        public string ConfirmationToken { get; set; }
    }
}