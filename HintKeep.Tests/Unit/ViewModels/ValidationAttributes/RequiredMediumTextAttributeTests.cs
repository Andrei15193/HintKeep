using System.ComponentModel.DataAnnotations;
using HintKeep.ViewModels.ValidationAttributes;
using Xunit;

namespace HintKeep.Tests.Unit.ViewModels.ValidationAttributes
{
    public class RequiredMediumTextAttributeTests
    {
        private readonly RequiredMediumTextAttribute _requiredMediumTextAttribute = new RequiredMediumTextAttribute();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\r")]
        [InlineData("\n")]
        [InlineData("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456")]
        public void InvalidText(string invalidText)
        {
            var validationResult = _requiredMediumTextAttribute.GetValidationResult(invalidText, new ValidationContext(new object()) { MemberName = "TestMemberName" });
            Assert.NotNull(validationResult);
            Assert.Equal("validation.errors.invalidRequiredMediumText", validationResult.ErrorMessage);
            Assert.Equal(new[] { "TestMemberName" }, validationResult.MemberNames);
        }

        [Theory]
        [InlineData("1234567890")]
        [InlineData("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345")]
        public void ValidText(string validText)
            => Assert.Null(_requiredMediumTextAttribute.GetValidationResult(validText, new ValidationContext(new object()) { MemberName = "TestMemberName" }));
    }
}