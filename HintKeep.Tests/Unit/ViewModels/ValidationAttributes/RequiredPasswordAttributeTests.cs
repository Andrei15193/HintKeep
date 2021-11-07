using System.ComponentModel.DataAnnotations;
using HintKeep.ViewModels.ValidationAttributes;
using Xunit;

namespace HintKeep.Tests.Unit.ViewModels.ValidationAttributes
{
    public class RequiredPasswordAttributeTests
    {
        private readonly RequiredPasswordAttribute _requiredPasswordAttribute = new RequiredPasswordAttribute();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\r")]
        [InlineData("\n")]
        [InlineData("pass")]
        [InlineData("password")]
        [InlineData("password$")]
        [InlineData("passWORD")]
        [InlineData("pa$$word")]
        [InlineData("pa$$w0rd")]
        [InlineData("password123")]
        [InlineData("p4$")]
        public void InvalidText(string invalidPassword)
        {
            var validationResult = _requiredPasswordAttribute.GetValidationResult(invalidPassword, new ValidationContext(new object()) { MemberName = "TestMemberName" });
            Assert.NotNull(validationResult);
            Assert.Equal("validation.errors.invalidRequiredPassword", validationResult.ErrorMessage);
            Assert.Equal(new[] { "TestMemberName" }, validationResult.MemberNames);
        }

        [Theory]
        [InlineData("P4$$word")]
        [InlineData(" password123PASSWORD")]
        public void ValidText(string validPassword)
            => Assert.Null(_requiredPasswordAttribute.GetValidationResult(validPassword, new ValidationContext(new object()) { MemberName = "TestMemberName" }));
    }
}