using HintKeep.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace HintKeep.Tests.Integration
{
    public static class TestSecurityServiceExtensions
    {
        public static WebApplicationFactory<TEntryPoint> WithSecurityService<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory)
            where TEntryPoint : class
            => webApplicationFactory.WithSecurityService(out var _);

        public static WebApplicationFactory<TEntryPoint> WithSecurityService<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, out ISecurityService securityService)
            where TEntryPoint : class
        {
            var securityServiceSubstitute = securityService = Substitute.For<ISecurityService>();
            return webApplicationFactory.WithWebHostBuilder(
                configuration => configuration.ConfigureTestServices(
                    services => services.AddSingleton(securityServiceSubstitute)
                )
            );
        }
    }
}