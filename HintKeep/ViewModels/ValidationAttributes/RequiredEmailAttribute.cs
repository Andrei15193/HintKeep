using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace HintKeep.ViewModels.ValidationAttributes
{
    public class RequiredEmailAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
            => value is string emailAddress && !string.IsNullOrWhiteSpace(emailAddress) && MailAddress.TryCreate(emailAddress, string.Empty, out var _);

        public override string FormatErrorMessage(string name)
            => "validation.errors.invalidRequiredEmailAddress";
    }
}