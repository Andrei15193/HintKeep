namespace HintKeep.ViewModels.ValidationAttributes
{
    public class ConfirmationTokenAttribute : MediumTextAttribute
    {
        public override bool IsValid(object value)
            => base.IsValid(value) && value is string text && text.Length > 0;

        public override string FormatErrorMessage(string name)
            => "validation.errors.invalidConfirmationToken";
    }
}