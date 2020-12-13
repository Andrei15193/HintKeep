using System.Linq;

namespace HintKeep.ViewModels.ValidationAttributes
{
    public class PasswordAttribute : MediumTextAttribute
    {
        public override bool IsValid(object value)
            => base.IsValid(value) && value is string password && password.Length >= 8
            && password.Any(char.IsDigit) && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(@char => !char.IsLetterOrDigit(@char));

        public override string FormatErrorMessage(string name)
            => "validation.errors.invalidPassword";
    }
}