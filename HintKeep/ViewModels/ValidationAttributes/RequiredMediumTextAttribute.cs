using System.ComponentModel.DataAnnotations;

namespace HintKeep.ViewModels.ValidationAttributes
{
    public class RequiredMediumTextAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
            => value is string text && !string.IsNullOrWhiteSpace(text) && text.Length <= 255;

        public override string FormatErrorMessage(string name)
            => "validation.errors.invalidRequiredMediumText";
    }
}