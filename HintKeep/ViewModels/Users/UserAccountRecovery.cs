using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public record UserAccountRecovery(
        [RequiredEmail] string Email
    );
}