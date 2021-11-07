using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;

namespace HintKeep.ViewModels.ValidationAttributes
{
    public class RequiredPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
            => value is string password && !string.IsNullOrWhiteSpace(password) && password.Length >= 8 && password.Length <= 255
                && password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(@char => !char.IsControl(@char) && !char.IsLetterOrDigit(@char));

        public override string FormatErrorMessage(string name)
            => "validation.errors.invalidRequiredPassword";
    }
}