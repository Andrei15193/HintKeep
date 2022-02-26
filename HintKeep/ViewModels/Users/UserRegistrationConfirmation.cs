using HintKeep.ViewModels.ValidationAttributes;

namespace HintKeep.ViewModels.Users
{
    public record UserRegistrationConfirmation(
        [RequiredMediumText] string Token
    );
}