using System.ComponentModel.DataAnnotations;
using HintKeep.ViewModels.ValidationAttributes;
using Xunit;

namespace HintKeep.Tests.Unit.ViewModels.ValidationAttributes
{
    public class EmailAttributeTests
    {
        private readonly EmailAttribute _emailAttribute = new EmailAttribute();

        [Theory]
        [InlineData("")]
        [InlineData("invalid e-mail")]
        [InlineData("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456")]
        public void InvalidText(string invalidText)
        {
            var validationResult = _emailAttribute.GetValidationResult(invalidText, new ValidationContext(new object()) { MemberName = "TestMemberName" });
            Assert.NotNull(validationResult);
            Assert.Equal("validation.errors.invalidEmail", validationResult.ErrorMessage);
            Assert.Equal(new[] { "TestMemberName" }, validationResult.MemberNames);
        }

        [Theory]
        [InlineData("test-email-address@domain.tld")]
        [InlineData("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234@domain.tld")]
        public void ValidText(string invalidText)
            => Assert.Null(_emailAttribute.GetValidationResult(invalidText, new ValidationContext(new object()) { MemberName = "TestMemberName" }));
    }
}