using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public class UserLogin
    {
        [Email]
        public string Email { get; set; }

        [RequiredMediumText]
        public string Password { get; set; }
    }
}