using System.ComponentModel.DataAnnotations;
using HintKeep.ViewModels.ValidationAttributes;
using Xunit;

namespace HintKeep.Tests.Unit.ViewModels.ValidationAttributes
{
    public class MediumTextAttributeTests
    {
        private readonly MediumTextAttribute _mediumTextAttribute = new MediumTextAttribute();

        [Theory]
        [InlineData("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456")]
        public void InvalidText(string invalidText)
        {
            var validationResult = _mediumTextAttribute.GetValidationResult(invalidText, new ValidationContext(new object()) { MemberName = "TestMemberName" });
            Assert.NotNull(validationResult);
            Assert.Equal("validation.errors.invalidMediumText", validationResult.ErrorMessage);
            Assert.Equal(new[] { "TestMemberName" }, validationResult.MemberNames);
        }

        [Theory]
        [InlineData("")]
        [InlineData("1234567890")]
        [InlineData("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345")]
        public void ValidText(string invalidText)
            => Assert.Null(_mediumTextAttribute.GetValidationResult(invalidText, new ValidationContext(new object()) { MemberName = "TestMemberName" }));
    }
}