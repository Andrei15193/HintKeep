using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public class UserSignUp
    {
        [Email]
        public string Email { get; set; }

        [Password]
        public string Password { get; set; }
    }
}