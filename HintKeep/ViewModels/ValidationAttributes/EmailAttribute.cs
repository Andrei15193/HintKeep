using System.Net.Mail;

namespace HintKeep.ViewModels.ValidationAttributes
{
    public class EmailAttribute : MediumTextAttribute
    {
        public override bool IsValid(object value)
            => base.IsValid(value) && value is string email && !string.IsNullOrWhiteSpace(email) && MailAddress.TryCreate(email, out var _);

        public override string FormatErrorMessage(string name)
            => "validation.errors.invalidEmail";
    }
}