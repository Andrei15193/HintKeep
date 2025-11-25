using GraphQL;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace HintKeep.GraphQL.Definitions.Users;

internal static class HintKeepHttp
{
    public const string SessionTokenIdFormat = "D";
    public static string SessionTokenCookieName(Guid sessionTokenId)
        => "session-token-" + sessionTokenId.ToString(SessionTokenIdFormat);

    public const string DevSessionTokenCookieName = "dev-session-token";
    public const string SessionTicketCookieName = "session-ticket";
    public const string SessionIdHeaderName = "X-HintKeep-Session";

    public static string? GetHttpCookie(this IResolveFieldContext context, string name)
        => context.GetHttpCookieHandler().Get(name);

    public static void SetHttpCookie(this IResolveFieldContext context, string name, string value, DateTime expiration)
        => context.GetHttpCookieHandler().Set(name, value, expiration);

    public static void SetDevHttpCookie(this IResolveFieldContext context, string name, string value, DateTime expiration)
    {
        if (context.RequestServices!.GetRequiredService<IHostEnvironment>().IsDevelopment())
            context.SetHttpCookie(name, value, expiration);
    }

    internal static void SetHttpCookieHandler(this ExecutionOptions options, IHttpCookieHandler cookieHandler)
         => options.UserContext[nameof(IHttpCookieHandler)] = cookieHandler;

    private static IHttpCookieHandler GetHttpCookieHandler(this IResolveFieldContext context)
        => (IHttpCookieHandler)context.UserContext[nameof(IHttpCookieHandler)]!;


    /// <remarks>
    /// The <see cref="IHttpCookieHandler"/> interface was added to accomodate for integration tests,
    /// the test host uses the ASP.NET pipeline and in consequence its own <c>HttpContext</c> which
    /// is different than the Azure Functions HTTP objects.
    /// 
    /// This abstracts HTTP cookie handling allowing them to be added regardless of runtime.
    /// </remarks>
    internal interface IHttpCookieHandler
    {
        string? Get(string name);

        void Set(string name, string value, DateTime expiration);
    }

    internal class FunctionsHttpCookieHandler(HttpRequestData httpRequestData, HttpResponseData httpResponseData) : IHttpCookieHandler
    {
        public string? Get(string name)
            => httpRequestData
                .Cookies
                .SingleOrDefault(cookie => cookie.Name == name)
                ?.Value;

        public void Set(string name, string value, DateTime expiration)
            => httpResponseData.Headers.Add(
                HeaderNames.SetCookie,
                new SetCookieHeaderValue(name, value)
                {
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true,
                    Expires = expiration
                }.ToString()
            );
    }
}