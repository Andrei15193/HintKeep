using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public class UserPasswordReset
    {
        [RequiredMediumText]
        public string Token { get; set; }

        [RequiredPassword]
        public string Password { get; set; }
    }
}