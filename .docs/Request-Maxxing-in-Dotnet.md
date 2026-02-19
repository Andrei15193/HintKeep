# HintKeep Stories: Request-Maxxing in Dotnet

This is a typical setup for a modern backend, handling queries or commands through requests, generally this would be done using MediatR however they have joined a somewhat current trend of adding licensing to a once completely free and beloved library.

What HintKeep uses is extremely simple, there will be no MediatR because there is no need, a homemade request handler is fairly basic and there is no need to add a dependency to a NuGet package. A request handler interface is defined, this will help identify implementations and register them.

```c#
public interface IRequestHandler<TRequest, TResult>
{
    ValueTask<TResult> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}
```

```c#
var requestHandlers =
    from type in typeof(Program).Assembly.DefinedTypes
    where type.IsClass && !type.IsAbstract
    from implementedInterface in type.ImplementedInterfaces
    where
        implementedInterface.IsConstructedGenericType
        && implementedInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
    select (implementedInterface, type);

foreach (var (requestHandlerInterface, requestHandlerImplementation) in requestHandlers)
    services.AddScoped(requestHandlerInterface, requestHandlerImplementation);
```

That is all that is needed, multiple assemblies can be added for lookup in case they are split in multiple projects.

## Register & Login Use Case

A common use case in applications, users can register and afterwards they can log into the application, upon registration the user is logged in automatically. Authentication is done using JSON Web Tokens. Some applications rely on 3rd party identity providers, however HintKeep has its own infrastructure for this which is detailed in a separate post.

Following the scenario, when a user registers they get their username checked for uniqueness and get a unique ID if all goes well, then a JSON Web Token is generated for their current session and returned to the user, all good.

Following the 2nd use case, a user logs into the application, they get their username and password checked for a match, if all goes well a JSON Web Token is generated for their session.

Wait a minute, in both cases a JWT is generated and it needs to be the same to ensure consistent behavior between the two use cases. If one claim is added to one, it needs to be added to the other, expiration needs to be the same.

How would this be solved? Quite simply, extract the common part, the one that generates the token into a separate _service_ which is an object exposing one or more methods for a specific purpose. Now we have the common part in one place, testable and reused in both request handlers.

## Maxing

This is common and quite useful, to have different helper/service types for different purposes and reusing them across the code base, it is a natural progression when writing code.

I was thinking a little bit more about this because now I need to think of a way of abstracting this a little bit, I do want my request handlers to be unit test ready which means I do need to have the ability to create fakes or use stubs for this new dependency. Besides this, I need to consider a directory structure for these _services_, place both the interface and the implementation and on top of this, register them accordingly.

This may not seem as much of a problem, just scope the lookup by namespace, maybe consider adding the ability to easily search other assemblies/projects if the code gets split to separate _services_ from _request handlers_.

On the other hand, I already have an infrastructure set in place that does exactly that. It takes a request, processes it and then produces a result. Why do I need _services_?

I can just as easily extract the common part that generates JSON Web Tokens in a 3rd request handler and have it invoked from the other 2. This reduces _[bad]_ code duplication, maintains unit test readiness and there is no need to think about how _services_ are structured and registered with DI, it is already done.

This is where the idea of **Request Maxing** came to mind: everything goes through a request handler. The only helper types would be extension methods to help with the infrastructure as we will see shortly.

## Implicit vs Explicit

Now we are stepping into complex territory, should request handlers be called in an implicit or explicit way? What do I mean by this, should the register user request handler explicitly ask for a request handler that knows how to generate the JSON Web Token, or should there be a "request dispatcher" to which I pass a request instance and get the result?

Keep in mind that each handler can do only one thing and one thing only, there is no option to overload methods or simply define more, everything goes through this limited definition of `HandleAsync`. Having multiple dependent handlers can lead to a long list of dependencies making the latter option more convenient.

Is "request dispatcher" evil? I would say so. Imagine you are going through a complex request handler that sometimes performs 5 other dispatches to get the final result. How difficult would it be to follow that code? What about writing tests for it? Instead of knowing what the dependencies are from the get go, you have to go through the implementation to even know what these dependencies are.

Besides all this, there is no help from dependency resolution with regards to circular dependencies. When dependencies are explicitly requested on the constructor, the dependency resolution will flag a circular dependency on instantiation rather than on execution. Simply creating an instance of a request handler will automatically let you know if you have a circular dependency.

Granted, HintKeep is unlikely to reach such heights, it is a fairly simple application, however the consideration goes in general. It is a trade-off when picking between implicit and explicit dependencies, or a mix between the two.

For the reasons listed above HintKeep is going the explicit route meaning that any dependent request handler is requested on the [primary] constructor regardless of how long the list gets. This will keep dependencies clear at all times and if I end up with a list that is too long, maybe there is something that I should look at rather than let it fester. This can be used as an indication when things needs to be further investigated and potentially simplified.

## Unknown Results

Everything looks great so far, except, when executing a request handler there is no way of knowing what the result will be, unless we check implementations. This is inconvenient, it would be great to know what the result of a request is to know the request handler is matched instead of trying to guess which one would be a good fit. It is good to leverage language support where possible.

For this, I have defined a marker interface, very original of me I know. Using generic constraints the result is enforced throughout the infrastructure.

```c#
public interface IRequest<out TResult>
{
}
```

```c#
public interface IRequestHandler<in TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    ValueTask<TResult> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}
```

To be fair, this is looking more and more like MediatR, most of the ideas I did get from there so it checks out. The one place I did consider using an alternate naming pattern is for the result. When we perform a request we generally get a _response_ rather than a _result_, take HTTP for instance. To avoid confusion between HTTP request handling and this type of request handling I went with result.

Using generics, we bound requests to their results, handlers follow this and any mismatch will be flagged by the compiler, this is looking great!

## Validation

I forgot how many times I tried to enter invalid input, of which I was certain that it was valid, only to argue with the screen followed by a brief moment of calm where I would actually read what it is telling me only to agree that indeed I am trying to type in my username instead of my email.

Whenever dealing with something that can be entered by a user, there _must_ be validation. We cannot trust humans, and we cannot trust bots either if we are being honest. We need to check what is being provided to ensure both consistency of our data, as well as behavior of our request handlers.

Dotnet already has support for this through their data validation attributes and `IValidatableObject` interface, this can be leveraged for requests as well. Granted, there should be stricter checks for all fields, such as minimum length, strong password requirements and a valid email address, but the example will suffice.

Instead of using classes to define requests, and results, I went with records just for their simplicity and because they make sense as well. A request is defined only by its data, it has no identity, two requests that contain the same data are the same request even if they are different instances.

```c#
public record RegisterRequest(
    [property: Required(ErrorMessage = "A username is required.")]
    string Username,

    [property: Required(ErrorMessage = "A password is required.")]
    string Password,

    [property: Required(ErrorMessage = "An email address is required.")]
    string EmailAddress
) :
    IRequest<AuthenticationResult>;
```

The `property` keyword here is mandatory, the dotnet validator only looks at properties and without specifying this the attribute would get set on the constructor parameters rather than the generated properties.

The next step is invoking the validator, this can be done in a controller or GraphQL mutation. If all fields are valid the handler is called, otherwise a list of errors is returned.

```c#
var request = new RegisterRequest("username", "password", "email address");
var validationResults = new List<ValidationResult>();
var validationContext = new ValidationContext(request);
// from System.ComponentModel.DataAnnotations
if (
    Validator.TryValidateObject(
        request,
        validationContext,
        validationResults,
        validateAllProperties: true
    )
    && validationResults.Count == 0
)
{
    // it's all good
}
else
{
    // oops
}
```

## Extensions

In the beginning I made an exception to request maxing and that was extension methods. If we are to strictly follow this pattern then whenever we would want to validate a request we would need a separate handler just for that, however this is unnecessary as it would make code rather more complicated than easier to follow.

A useful utility could be defined to ensure a request is valid, this will allow the use of data validation attributes at any level and before making a call to a dependent handler.

To simplify code even further, the extension method can return the provided instance allowing us to both generate, validate and pass a request to its respective handler in one go. One small issue though, the base `IRequest` interface is generic and defining an extension method constrained to it would always need to have the result type specified, it cannot be automatically resolved by the compiler.

To solve this, we can define yet a 2nd base interface that removes the result binding, the purpose is to enable writing extension methods on requests alone.

```c#
public interface IRequest
{
}

public interface IRequest<out TResult> : IRequest
{
}
```

```c#
public static class RequestExtensions
{
    public static TRequest EnsureValid<TRequest>(this TRequest request)
        where TRequest : IRequest
    {
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, validateAllProperties: true);

        return request;
    }
}
```

The extension method is now ready, it will either forward the instance or throw an exception. When generating requests from another handler they should generally be valid. There is little feedback that can be provided to a user at this point as we have gotten past the initial request.

```c#
IRequestHandler<RegisterRequest, AuthenticationResult> registerRequestHandler;

await registerRequestHandler.HandleAsync(
    new RegisterRequest("username", "password", "email address").EnsureValid(),
    cancellationToken
);
```

## Conclusion

A request-only approach does check a lot of boxes when it comes to code isolation, maintenance and reusability, on the other hand splitting code like this can make it difficult to know which requests are intended as "entry points" and which are there to reduce code duplication. This could be resolved through an additional marker interface or attribute.

A trade-off that needs to be considered is between explicit vs implicit dependent request handlers, having a dispatcher seems very convenient, however it can hide a significant part of the complexity of a handler making it harder to follow, test and safeguard against circular dependencies.

HintKeep is a small project, just about any approach will work, however there is a high likelihood that _request maxing_ can work even on large projects as long as a clear structure is implemented along side request handlers, having an unstructured mix of types makes code much harder to navigate.

As a final thought, I do want to include dependency chains in the wiki, preferably have this generated automatically. Having a list of request handlers and if they have any such subsequent dependencies is useful for a high-level overview or just to see how different parts communicate with one another.