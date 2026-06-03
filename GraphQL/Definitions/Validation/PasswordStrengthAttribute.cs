using System.ComponentModel.DataAnnotations;

namespace HintKeep.GraphQL.Definitions.Validation;

public class PasswordStrengthAttribute : ValidationAttribute
{
    public PasswordStrengthAttribute()
        : base(() => "Password does not meet strength requirements.")
    {
    }

    public override bool IsValid(object? value)
        => (
            value is string stringValue
            && stringValue.Length >= 8
            && stringValue.Any(char.IsLetter)
            && stringValue.Any(char.IsDigit)
            && stringValue.Any(char.IsUpper)
            && stringValue.Any(char.IsLower)
            && stringValue.Any(@char => !char.IsLetterOrDigit(@char))
        );
}