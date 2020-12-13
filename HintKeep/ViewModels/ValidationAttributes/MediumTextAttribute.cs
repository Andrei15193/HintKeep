using System.ComponentModel.DataAnnotations;

namespace HintKeep.ViewModels.ValidationAttributes
{
    public class MediumTextAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
            => value is string text && text.Length <= 255;

        public override string FormatErrorMessage(string name)
            => "validation.errors.invalidMediumText";
    }
}