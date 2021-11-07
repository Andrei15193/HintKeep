using System.ComponentModel.DataAnnotations;
using HintKeep.ViewModels.ValidationAttributes;
using Xunit;

namespace HintKeep.Tests.Unit.ViewModels.ValidationAttributes
{
    public class RequiredEmailAttributeTests
    {
        private readonly RequiredEmailAttribute _requiredEmailAttribute = new RequiredEmailAttribute();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\r")]
        [InlineData("\n")]
        [InlineData("invalid-email-address")]
        public void InvalidText(string invalidEmailAddress)
        {
            var validationResult = _requiredEmailAttribute.GetValidationResult(invalidEmailAddress, new ValidationContext(new object()) { MemberName = "TestMemberName" });
            Assert.NotNull(validationResult);
            Assert.Equal("validation.errors.invalidRequiredEmailAddress", validationResult.ErrorMessage);
            Assert.Equal(new[] { "TestMemberName" }, validationResult.MemberNames);
        }

        [Theory]
        [InlineData("test@domain.com")]
        [InlineData("some-address@some-domain.some-top-level-domain")]
        public void ValidText(string validEmailAddress)
            => Assert.Null(_requiredEmailAttribute.GetValidationResult(validEmailAddress, new ValidationContext(new object()) { MemberName = "TestMemberName" }));
    }
}