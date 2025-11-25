using System.Text.Json.Nodes;
using Xunit.Gherkin.Quick;

namespace HintKeep.GraphQL.Features;

public class Authentication(HintKeepWebApplicationFactory factory) : HintKeepFeature(factory)
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
    public void AndGivenSeenText(string text)
    {
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