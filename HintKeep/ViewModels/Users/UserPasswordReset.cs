using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public record UserPasswordReset(
        [RequiredEmail] string Email,
        [RequiredMediumText] string Token,
        [RequiredPassword] string Password
    );
}