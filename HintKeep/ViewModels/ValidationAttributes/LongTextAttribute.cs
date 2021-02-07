using System.ComponentModel.DataAnnotations;

namespace HintKeep.ViewModels.ValidationAttributes
{
    public class LongTextAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
            => value is null || (value is string text && text.Length <= 1000);

        public override string FormatErrorMessage(string name)
            => "validation.errors.invalidLongText";
    }
}