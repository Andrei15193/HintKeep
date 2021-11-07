using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public class UserRegistration
    {
        [RequiredEmail]
        public string Email { get; set; }

        [RequiredMediumText]
        public string Hint { get; set; }

        [RequiredPassword]
        public string Password { get; set; }
    }
}