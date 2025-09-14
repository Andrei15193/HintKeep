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

    public static void SetHttpResponseCookie(this IResolveFieldContext context, string name, string value, DateTime expiration)
    {
        var httpResponseData = context.GetHttpResponseData();

        httpResponseData.Headers.Add(
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

    public static void SetDevHttpResponseCookie(this IResolveFieldContext context, string name, string value, DateTime expiration)
    {
        var environment = context.RequestServices!.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
            context.SetHttpResponseCookie(name, value, expiration);
    }

    internal static HttpRequestData GetHttpRequestData(this IResolveFieldContext context)
        => (HttpRequestData)context.UserContext[nameof(HttpRequestData)]!;

    internal static void SetHttpRequestData(this ExecutionOptions options, HttpRequestData httpRequestData)
        => options.UserContext[nameof(HttpRequestData)] = httpRequestData;

    internal static HttpResponseData GetHttpResponseData(this IResolveFieldContext context)
        => (HttpResponseData)context.UserContext[nameof(HttpResponseData)]!;

    internal static void SetHttpResponseData(this ExecutionOptions options, HttpResponseData httpResponseData)
        => options.UserContext[nameof(HttpResponseData)] = httpResponseData;
}