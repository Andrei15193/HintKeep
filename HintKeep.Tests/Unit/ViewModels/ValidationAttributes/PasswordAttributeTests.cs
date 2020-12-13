using System.ComponentModel.DataAnnotations;
using HintKeep.ViewModels.ValidationAttributes;
using Xunit;

namespace HintKeep.Tests.Unit.ViewModels.ValidationAttributes
{
    public class PasswordAttributeTests
    {
        private readonly PasswordAttribute _passwordAttribute = new PasswordAttribute();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(1)]
        [InlineData("test")]
        [InlineData("only lower case letters")]
        [InlineData("ONLY UPPER CASE LETTERS")]
        [InlineData("07734")]
        [InlineData("only LETTERS")]
        [InlineData("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456")]
        public void InvalidPassword(object invalidPassword)
        {
            var validationResult = _passwordAttribute.GetValidationResult(invalidPassword, new ValidationContext(new object()) { MemberName = "TestMemberName" });
            Assert.NotNull(validationResult);
            Assert.Equal("validation.errors.invalidPassword", validationResult.ErrorMessage);
            Assert.Equal(new[] { "TestMemberName" }, validationResult.MemberNames);
        }

        [Theory]
        [InlineData("a Valid password1")]
        public void ValidPassword(object valid)
            => Assert.Null(_passwordAttribute.GetValidationResult(valid, new ValidationContext(new object()) { MemberName = "TestMemberName" }));
    }
}