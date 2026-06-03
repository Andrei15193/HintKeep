using HintKeep.GraphQL.Definitions.Users.Accounts;
using Xunit.Gherkin.Quick;

namespace HintKeep.GraphQL.Features;

public class RegisterUserAccount : HintKeepFeature
{
    private IPageContext _pageContext = null!;

    [Given("the landing page")]
    public void GivenTheLandingPage()
        => _pageContext = new LandingPageContext();

    [Given("the sign up page")]
    public void GivenTheSignUpPage()
        => _pageContext = new SignUpPageContext(this);

    [Given("the {string} field filled with {string}")]
    [And("the {string} field filled with {string}")]
    public void GivenFieldValue(string fieldId, string fieldValue)
        => _pageContext.SetFieldValue(fieldId, fieldValue);

    [Given("the {string} field filled with {int} characters")]
    [And("the {string} field filled with {int} characters")]
    public void GivenFieldValueLength(string fieldId, int fieldValueLength)
        => _pageContext.SetFieldValue(fieldId, new string('a', fieldValueLength));

    [Given("a user with {string} username and {string} email already exists")]
    [But("a user with {string} username and {string} email already exists")]
    public Task GivenExistingUser(string username, string emailAddress)
        => DispatchRequestAsync(new RegisterUserAccountRequest(
            Username: username,
            Password: "pa$$WORD123",
            Hint: "Hint",
            EmailAddress: emailAddress
        ));

    [When("I click on the {string} button")]
    public Task WhenIClickButton(string buttonId)
        => _pageContext.ClickButtonAsync(buttonId);

    [Then("I can see the {string} page")]
    public void ThenISeePage(string pageTitle)
    {
    }

    [Then("I can see the {string} field")]
    [And("I can see the {string} field")]
    public void ThenISeeField(string fieldId)
    {
    }

    [Then("I can see the {string} button")]
    [And("I can see the {string} button")]
    public void ThenISeeButton(string buttonId)
    {
    }

    [Then("the current user is {string}")]
    [And("the current user is {string}")]
    public void ThenCurrentUser(string username)
    {
        Assert.NotNull(DataResult);
        Assert.Equal(username, DataResult["username"]!.GetValue<string>());
        Assert.NotNull(DataResult["userId"]!.GetValue<string>());
        Assert.NotNull(DataResult["sessionId"]!.GetValue<string>());
        Assert.NotNull(DataResult["sessionRenewTicket"]!.GetValue<string>());
    }

    [Then("I can see {string} error message for the {string} field")]
    [And("I can see {string} error message for the {string} field")]
    public void ThenFieldError(string errorMessage, string fieldId)
        => Assert.Equal(errorMessage, _pageContext.GetFieldError(fieldId));

    [Then("I can see the landing page")]
    public void ThenSuccessfulResult()
        => Assert.Multiple(
            () =>
            {
                if (RawResult is null)
                    Assert.Null(DataResult);
                else
                {
                    Assert.NotNull(DataResult);
                    Assert.Multiple(
                        () => Assert.Contains("username", DataResult.AsObject()),
                        () => Assert.Contains("userId", DataResult.AsObject()),
                        () => Assert.Contains("sessionId", DataResult.AsObject()),
                        () => Assert.Contains("sessionRenewTicket", DataResult.AsObject())
                    );
                }
            },
            () => Assert.Null(ErrorResult)
        );

    private interface IPageContext
    {
        Task ClickButtonAsync(string buttonId);
        void SetFieldValue(string fieldId, string fieldValue);
        string? GetFieldError(string fieldId);
    }

    private class LandingPageContext : IPageContext
    {
        public Task ClickButtonAsync(string buttonText)
            => Task.CompletedTask;

        public void SetFieldValue(string fieldId, string fieldValue)
        {
        }

        public string? GetFieldError(string fieldId)
            => null;
    }

    private class SignUpPageContext(HintKeepFeature feature) : IPageContext
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _hint = string.Empty;
        private string _emailAddress = string.Empty;

        public Task ClickButtonAsync(string buttonId)
            => buttonId switch
            {
                "sign up" => feature.ExecuteQueryAsync(@"
                        mutation($username: String!, $password: String!, $hint: String!, $emailAddress: String!) {
                            registerUserAccount(username: $username, password: $password, hint: $hint, emailAddress: $emailAddress) {
                                userId
                                sessionId
                                sessionRenewTicket
                                username
                            }
                        }
                    ",
                    new
                    {
                        username = _username,
                        password = _password,
                        hint = _hint,
                        emailAddress = _emailAddress
                    }
                ),
                _ => Task.CompletedTask
            };

        public void SetFieldValue(string fieldId, string fieldValue)
        {
            switch (fieldId)
            {
                case "username":
                    _username = fieldValue;
                    break;

                case "password":
                    _password = fieldValue;
                    break;

                case "hint":
                    _hint = fieldValue;
                    break;

                case "email":
                    _emailAddress = fieldValue;
                    break;
            }
        }

        public string? GetFieldError(string fieldId)
            => fieldId switch
            {
                "username" => feature.FieldsErrorResult!.AsObject()["username"]?.GetValue<string>(),
                "password" => feature.FieldsErrorResult!.AsObject()["password"]?.GetValue<string>(),
                "hint" => feature.FieldsErrorResult!.AsObject()["hint"]?.GetValue<string>(),
                "email" => feature.FieldsErrorResult!.AsObject()["emailAddress"]?.GetValue<string>(),
                _ => throw new ArgumentException($"Unhandled '{fieldId}' field ID.")
            };
    }
}