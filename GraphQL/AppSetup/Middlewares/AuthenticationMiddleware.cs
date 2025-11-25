using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using HintKeep.GraphQL.Definitions.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.GraphQL.AppSetup.Middlewares;

internal class AuthenticationMiddleware : IFunctionsWorkerMiddleware, IMiddleware
{
    public Task Invoke(FunctionContext functionContext, FunctionExecutionDelegate next)
    {
        var httpContext = functionContext.GetHttpContext();

        if (httpContext is null)
            return next(functionContext);
        else
            return ((IMiddleware)this).InvokeAsync(httpContext, new RequestDelegate(_ => next(functionContext)));
    }

    /// <remarks>
    /// The <see cref="IMiddleware"/> interface is only implemented to facilitate integration testing.
    /// </remarks>
    async Task IMiddleware.InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var environment = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<AuthenticationMiddleware>>();

        var jwtSecurityTokenHandler = httpContext.RequestServices.GetRequiredService<JwtSecurityTokenHandler>();
        var tokenValidationParameters = httpContext.RequestServices.GetRequiredKeyedService<TokenValidationParameters>(ServiceKeys.SessionToken);

        logger.LogInformation("Authenticating user.");

        Exception? authException = null;
        var sessionToken = string.Empty;

        var hintKeepSessionHeaderValues = httpContext.Request.Headers[HintKeepHttp.SessionIdHeaderName];
        if (hintKeepSessionHeaderValues.Count == 0)
        {
            if (environment.IsDevelopment())
                httpContext.Request.Cookies.TryGetValue(HintKeepHttp.DevSessionTokenCookieName, out sessionToken);
        }
        else
        {
            sessionToken = hintKeepSessionHeaderValues
                .Select(sessionIdString => (
                    Guid.TryParseExact(sessionIdString, HintKeepHttp.SessionTokenIdFormat, out var sessionId)
                    ? httpContext.Request.Cookies.TryGetValue(HintKeepHttp.SessionTokenCookieName(sessionId), out var sessionToken)
                        ? sessionToken
                        : null
                    : null
                ))
                .FirstOrDefault(sessionId => sessionId is not null);
        }

        if (string.IsNullOrWhiteSpace(sessionToken))
            logger.LogInformation("User is not authenticated, proceeding as anonymous.");
        else
            try
            {
                httpContext.User = jwtSecurityTokenHandler.ValidateToken(sessionToken, tokenValidationParameters, out var _);
                logger.LogInformation("User is authenticated with '{userId}' user ID.", httpContext.User.FindFirstValue(HintKeepClaims.UserId));
            }
            catch (SecurityTokenException securityTokenException)
            {
                logger.LogWarning(securityTokenException, "Invalid JSON Web Token '{jwt}'", sessionToken);
                authException = securityTokenException;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Invalid JSON Web Token '{jwt}'.", sessionToken);
                authException = exception;
            }

        if (authException is null)
            await next(httpContext);
        else
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            if (environment.IsDevelopment())
            {
                httpContext.Response.Headers.ContentType = "application/text; charset=utf-8";
                await httpContext.Response.WriteAsync(authException.ToString(), Encoding.UTF8);
            }
        }
    }
}