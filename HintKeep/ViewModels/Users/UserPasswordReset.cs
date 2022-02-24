using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public class UserPasswordReset
    {
        [RequiredEmail]
        public string Email { get; set; }

        [RequiredMediumText]
        public string Token { get; set; }

        [RequiredPassword]
        public string Password { get; set; }
    }
}