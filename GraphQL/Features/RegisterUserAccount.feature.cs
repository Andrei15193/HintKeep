using HintKeep.GraphQL.Definitions.Users.Accounts;
using HintKeep.GraphQL.Features.PageContexts;
using Xunit.Gherkin.Quick;

namespace HintKeep.GraphQL.Features;

public class RegisterUserAccount : HintKeepFeature
{
    [Given("the landing page")]
    [And("the landing page")]
    public void GivenTheLandingPage()
        => ApplicationContext.PageContext = new LogInPageContext();

    [Given("the sign up page")]
    [And("the sign up page")]
    public void GivenTheSignUpPage()
        => ApplicationContext.PageContext = new SignUpPageContext();

    [Given("a user with {string} username and {string} email")]
    [And("a user with {string} username and {string} email")]
    public Task GivenUser(string username, string email)
        => DispatchRequestAsync(new RegisterUserAccountRequest(
            Username: username,
            Password: "pa$$WORD123",
            Hint: "Hint",
            Email: email
        ));

    [When("I enter {string} in the {string} field")]
    [And("I enter {string} in the {string} field")]
    public void WhenIEnterValueInField(string value, string field)
        => ApplicationContext.PageContext = ApplicationContext.PageContext with
        {
            FormData = new Dictionary<string, object?>(ApplicationContext.PageContext.FormData)
            {
                [field] = value
            }
        };

    [When("I enter {int} characters in the {string} field")]
    [And("I enter {int} characters in the {string} field")]
    public void WhenIEnterNumberOfCharactersInField(int characterCount, string field)
        => WhenIEnterValueInField(new string('c', characterCount), field);

    [When("I enter {int} character email address in the {string} field")]
    [And("I enter {int} character email address in the {string} field")]
    public void WhenIEnterNumberOfCharactersEmailInField(int characterCount, string field)
        => WhenIEnterValueInField(new string('c', characterCount - "@email.com".Length) + "@email.com", field);

    [When("I press the {string} link")]
    [And("I press the {string} link")]
    public async Task WhenIPressLink(string link)
    {
        if (ApplicationContext.PageContext.LinkActions.TryGetValue(link, out var linkAction))
            await linkAction(ApplicationContext);
        else
            Assert.Fail($"Unknown '{link}' link on '{ApplicationContext.PageContext.Name}' page.");
    }

    [When("I press the {string} button")]
    [And("I press the {string} button")]
    public async Task WhenIPressButton(string button)
    {
        if (ApplicationContext.PageContext.ButtonActions.TryGetValue(button, out var linkAction))
            await linkAction(ApplicationContext);
        else
            Assert.Fail($"Unknown '{button}' button on '{ApplicationContext.PageContext.Name}' page.");
    }

    [When("I press the sign up button")]
    [And("I press the sign up button")]
    public Task WhenIPressSignUpButton()
        => WhenIPressButton("sign up");

    [Then("I am on the {string} page")]
    [And("I am on the {string} page")]
    public void ThenIAmOnPage(string page)
        => Assert.Equal(
            page,
            ApplicationContext.PageContext.Name,
            ignoreCase: true,
            ignoreWhiteSpaceDifferences: true,
            ignoreLineEndingDifferences: true
        );

    [Then("the current user is {string}")]
    [And("the current user is {string}")]
    public void ThenCurrentUserIs(string username)
    {
        Assert.NotNull(ApplicationContext.CurrentUser);
        Assert.Equal(
            username,
            ApplicationContext.CurrentUser.Username,
            ignoreCase: true,
            ignoreWhiteSpaceDifferences: false,
            ignoreAllWhiteSpace: false,
            ignoreLineEndingDifferences: false
        );
    }

    [Then("I have the {string} field")]
    [And("I have the {string} field")]
    public void ThenIHaveField(string field)
        => Assert.Contains(field, ApplicationContext.PageContext.FormData);

    [Then("I have {string} error message for the {string} field")]
    [And("I have {string} error message for the {string} field")]
    public void ThenIHaveErrorForField(string errorMessage, string field)
        => Assert.Multiple(
            () => Assert.Contains(field, ApplicationContext.PageContext.FormData),
            () => Assert.Contains(field, ApplicationContext.PageContext.FormErrorData),
            () => Assert.Equal(
                errorMessage,
                ApplicationContext.PageContext.FormErrorData[field],
                ignoreCase: false,
                ignoreWhiteSpaceDifferences: false,
                ignoreLineEndingDifferences: true
            )
        );

    [Then("I have the {string} link")]
    [And("I have the {string} link")]
    public void ThenIHaveLink(string link)
        => Assert.Contains(link, ApplicationContext.PageContext.LinkActions);

    [Then("I have the {string} button")]
    [And("I have the {string} button")]
    public void ThenIHaveButton(string button)
        => Assert.Contains(button, ApplicationContext.PageContext.ButtonActions);
}