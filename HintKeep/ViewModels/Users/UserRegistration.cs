using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public record UserRegistration(
        [RequiredEmail] string Email,
        [RequiredMediumText] string Hint,
        [RequiredPassword] string Password
    );
}