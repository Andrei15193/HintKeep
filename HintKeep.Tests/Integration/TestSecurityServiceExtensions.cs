using HintKeep.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HintKeep.Tests.Integration
{
    public static class TestSecurityServiceExtensions
    {
        public static WebApplicationFactory<TEntryPoint> WithSecurityService<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory)
            where TEntryPoint : class
            => webApplicationFactory.WithSecurityService(out var _);

        public static WebApplicationFactory<TEntryPoint> WithSecurityService<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, out Mock<ISecurityService> securityService)
            where TEntryPoint : class
        {
            var securityServiceMock = new Mock<ISecurityService>();
            securityService = securityServiceMock;
            return webApplicationFactory.WithWebHostBuilder(
                configuration => configuration.ConfigureTestServices(
                    services => services.AddSingleton<ISecurityService>(securityServiceMock.Object)
                )
            );
        }
    }
}