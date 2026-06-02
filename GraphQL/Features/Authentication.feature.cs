using System.Text.Json.Nodes;
using HintKeep.GraphQL.Definitions.Users.Accounts;
using Xunit.Gherkin.Quick;

namespace HintKeep.GraphQL.Features;

public class Authentication : HintKeepFeature
{
    private JsonNode? _result = null;

    [Given("the landing page")]
    public void GivenTheLandingPage()
    {
    }

    [And("I click on {string}")]
    public void AndGivenTextClick(string text)
    {
    }

    [And("I see {string}")]
    [Then("I see {string}")]
    public void AndGivenSeenText(string text)
    {
    }

    [And("there is an existing user with {string} username, {string} password and {string} hint")]
    public async Task AndGivenExistingUser(string username, string password, string hint)
    {
        await DispatchRequestAsync(new RegisterUserAccountRequest(username, password, hint, $"{username}@email.com"));
    }

    [When("I enter {string} for {string}")]
    public void WhenEnteredText(string value, string inputLabel)
    {
    }

    [And("I enter {string} for {string}")]
    public void AndWhenEnteredText(string value, string inputLabel)
    {
    }

    [Then("I see the {string} error message for {string}")]
    public async Task ThenLoginFails(string error, string inputLabel)
    {
        _result = await ExecuteQueryAsync(
            @"mutation($username: String! $password: String!) {
                authenticate(username: $username password: $password) {
                    userId
                    sessionId
                    sessionRenewTicket
                    username
                }
            }",
            new
            {
                username = "test",
                password = "test"
            }
        );

        Assert.NotNull(_result["errors"]);
        Assert.NotEmpty(_result["errors"]?.AsArray()!);
        Assert.Equal("Input validation failed", _result["errors"]?.AsArray()[0]?.AsObject()["message"]?.GetValue<string>());
    }
}