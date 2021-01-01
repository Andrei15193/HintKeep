using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public class UserLogin
    {
        [Email]
        public string Email { get; set; }

        [Password]
        public string Password { get; set; }
    }
}