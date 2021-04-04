using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.Tests.Integration
{
    public static class TestAuthenticationExtensions
    {
        public const string TestAuthenticationScheme = JwtBearerDefaults.AuthenticationScheme + "TEST";

        public static WebApplicationFactory<TEntryPoint> WithAuthentication<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, string userId)
            where TEntryPoint : class
            => webApplicationFactory.WithWebHostBuilder(configuration =>
            {
                configuration.ConfigureTestServices(services =>
                {
                    services
                        .AddSingleton(new Session(userId))
                        .AddAuthorization(
                            options => options.DefaultPolicy = new AuthorizationPolicyBuilder()
                                .AddAuthenticationSchemes(TestAuthenticationScheme)
                                .RequireAssertion(context => true)
                                .Build()
                        )
                        .AddAuthentication(TestAuthenticationScheme)
                        .AddJwtBearer(TestAuthenticationScheme, options => { });
                });
            });
    }
}