using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public record UserAuthentication(
        [RequiredEmail] string Email,
        [RequiredMediumText] string Password
    );
}