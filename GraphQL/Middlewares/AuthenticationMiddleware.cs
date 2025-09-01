using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using HintKeep.GraphQL.Definitions.UserAccounts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.GraphQL.Middlewares;

public class AuthenticationMiddleware(
    IHostEnvironment environment,
    ILogger<AuthenticationMiddleware> logger,
    JwtSecurityTokenHandler jwtSecurityTokenHandler,
    TokenValidationParameters tokenValidationParameters
) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext is null)
            await next(context);
        else
        {
            logger.LogInformation("Authenticating user.");

            Exception? authException = null;
            if (httpContext.Request.Headers.Authorization.Count == 0)
                logger.LogInformation("User is not authenticated, proceeding as anonymous.");
            else
            {
                var jsonWebToken = httpContext.Request.Headers.Authorization
                    .Where(authorizationHeaderValue => !string.IsNullOrWhiteSpace(authorizationHeaderValue))
                    .Select(authorizationHeaderValue => authorizationHeaderValue!.Replace(JwtBearerDefaults.AuthenticationScheme + " ", string.Empty))
                    .First();
                try
                {
                    httpContext.User = jwtSecurityTokenHandler.ValidateToken(jsonWebToken, tokenValidationParameters, out var _);
                    logger.LogInformation("User is authenticated with '{userId}' user ID.", httpContext.User.FindFirstValue(HintKeepClaims.UserId));
                }
                catch (SecurityTokenException securityTokenException)
                {
                    logger.LogWarning(securityTokenException, "Invalid JSON Web Token '{jwt}'", jsonWebToken);
                    authException = securityTokenException;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Invalid JSON Web Token '{jwt}'.", jsonWebToken);
                    authException = exception;
                }
            }

            if (authException is null)
                await next(context);
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
}