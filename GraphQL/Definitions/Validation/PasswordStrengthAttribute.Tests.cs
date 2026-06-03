namespace HintKeep.GraphQL.Definitions.Validation;

public class PasswordStrengthAttributeTests
{
    [Theory]
    [InlineData("password", false)]
    [InlineData("aA1@", false)]
    [InlineData("aA1@!93", false)]
    [InlineData("aA1@!93a", true)]
    [InlineData("pa$$WORD123", true)]
    public void IsValid(string password, bool expectedIsValid)
    {
        var isValid = new PasswordStrengthAttribute().IsValid(password);

        if (expectedIsValid)
            Assert.True(isValid);
        else
            Assert.False(isValid);
    }
}