using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public class UserAccountRecovery
    {
        [RequiredEmail]
        public string Email { get; set; }
    }
}