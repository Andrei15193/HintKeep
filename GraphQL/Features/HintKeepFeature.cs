using System.Reflection;
using GraphQL;
using HintKeep.GraphQL.Definitions;
using HintKeep.GraphQL.Definitions.Users.Accounts;
using HintKeep.GraphQL.Features.Contexts;
using HintKeep.GraphQL.Features.PageContexts;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Gherkin.Quick;

namespace HintKeep.GraphQL.Features;

[FeatureFile(@".*\.feature", FeatureFilePathType.Regex)]
public class HintKeepFeature : Feature, IDisposable
{
    private readonly HintKeepWebApplicationFactory _factory;
    private readonly ApplicationContext _applicationContext;

    public HintKeepFeature()
    {
        _factory = new();
        _applicationContext = new(
            _factory.Services.GetRequiredService<IGraphQLTextSerializer>(),
            _factory.CreateClient()
        );
    }

    [Given("the landing page")]
    [And("the landing page")]
    public void GivenTheLandingPage()
        => _applicationContext.PageContext = new LogInPageContext();

    [Given("the sign up page")]
    [And("the sign up page")]
    public void GivenTheSignUpPage()
        => _applicationContext.PageContext = new SignUpPageContext();

    [Given("a user with {string} username and {string} email")]
    [And("a user with {string} username and {string} email")]
    public Task GivenUserWithEmail(string username, string email)
        => DispatchRequestAsync(new RegisterUserAccountRequest(
            Username: username,
            Password: $"passWORD$123 {Guid.NewGuid()}@email.com",
            Hint: "Hint",
            Email: email
        ));

    [Given("a user with {string} username and {string} password")]
    [And("a user with {string} username and {string} password")]
    public Task GivenUserWithPassword(string username, string password)
        => DispatchRequestAsync(new RegisterUserAccountRequest(
            Username: username,
            Password: password,
            Hint: "Hint",
            Email: $"{Guid.NewGuid()}@email.com"
        ));

    [When("I enter {string} in the {string} field")]
    [And("I enter {string} in the {string} field")]
    public void WhenIEnterValueInField(string value, string field)
        => _applicationContext.PageContext = _applicationContext.PageContext with
        {
            FormData = new Dictionary<string, object?>(_applicationContext.PageContext.FormData)
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
        if (_applicationContext.PageContext.LinkActions.TryGetValue(link, out var linkAction))
            await linkAction(_applicationContext);
        else
            Assert.Fail($"Unknown '{link}' link on '{_applicationContext.PageContext.Name}' page.");
    }

    [When("I press the {string} button")]
    [And("I press the {string} button")]
    public async Task WhenIPressButton(string button)
    {
        if (_applicationContext.PageContext.ButtonActions.TryGetValue(button, out var linkAction))
            await linkAction(_applicationContext);
        else
            Assert.Fail($"Unknown '{button}' button on '{_applicationContext.PageContext.Name}' page.");
    }

    [When("I press the login button")]
    [And("I press the login button")]
    public Task WhenIPressLoginButton()
        => WhenIPressButton("login");

    [When("I press the sign up button")]
    [And("I press the sign up button")]
    public Task WhenIPressSignUpButton()
        => WhenIPressButton("sign up");

    [Then("I am on the {string} page")]
    [And("I am on the {string} page")]
    public void ThenIAmOnPage(string page)
        => Assert.Equal(
            page,
            _applicationContext.PageContext.Name,
            ignoreCase: true,
            ignoreWhiteSpaceDifferences: true,
            ignoreLineEndingDifferences: true
        );

    [Then("the current user is {string}")]
    [And("the current user is {string}")]
    public void ThenCurrentUserIs(string username)
    {
        Assert.NotNull(_applicationContext.CurrentUser);
        Assert.Equal(
            username,
            _applicationContext.CurrentUser.Username,
            ignoreCase: true,
            ignoreWhiteSpaceDifferences: false,
            ignoreAllWhiteSpace: false,
            ignoreLineEndingDifferences: false
        );
    }

    [Then("I have the {string} field")]
    [And("I have the {string} field")]
    public void ThenIHaveField(string field)
        => Assert.Contains(field, _applicationContext.PageContext.FormData);

    [Then("I have {string} error message for the {string} field")]
    [And("I have {string} error message for the {string} field")]
    public void ThenIHaveErrorForField(string errorMessage, string field)
        => Assert.Multiple(
            () => Assert.Contains(field, _applicationContext.PageContext.FormData),
            () => Assert.Contains(field, _applicationContext.PageContext.FormErrorData),
            () => Assert.Equal(
                errorMessage,
                _applicationContext.PageContext.FormErrorData[field],
                ignoreCase: false,
                ignoreWhiteSpaceDifferences: false,
                ignoreLineEndingDifferences: true
            )
        );

    [Then("I have the {string} link")]
    [And("I have the {string} link")]
    public void ThenIHaveLink(string link)
        => Assert.Contains(link, _applicationContext.PageContext.LinkActions);

    [Then("I have the {string} button")]
    [And("I have the {string} button")]
    public void ThenIHaveButton(string button)
        => Assert.Contains(button, _applicationContext.PageContext.ButtonActions);

    internal Task DispatchRequestAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var resultType = requestType
            .GetInterfaces()
            .First(@interface => @interface.IsConstructedGenericType && typeof(IRequest<>).IsAssignableFrom(@interface.GetGenericTypeDefinition()))
            .GetGenericArguments()
            .Single();
        var requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
        var executeAsyncMethod = requestHandlerType.GetMethod(nameof(IRequestHandler<IRequest<object>, object>.ExecuteAsync), BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)!;

        var requestHandler = _factory.Services.GetRequiredService(requestHandlerType);

        var valueTaskResult = executeAsyncMethod.Invoke(requestHandler, [request.EnsureValid(), cancellationToken])!;
        var task = (Task)valueTaskResult
            .GetType()
            .GetMethod(nameof(ValueTask<object>.AsTask), BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
            !.Invoke(valueTaskResult, [])!;

        return task;
    }

    public virtual void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }
}