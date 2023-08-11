using HintKeep.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace HintKeep.Tests.Integration
{
    public static class TestEmailServiceExtensions
    {
        public static WebApplicationFactory<TEntryPoint> WithEmailService<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory)
            where TEntryPoint : class
            => webApplicationFactory.WithEmailService(out var _);

        public static WebApplicationFactory<TEntryPoint> WithEmailService<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, out IEmailService emailService)
            where TEntryPoint : class
        {
            var emailServiceSubstitute = emailService = Substitute.For<IEmailService>();
            return webApplicationFactory.WithWebHostBuilder(
                configuration => configuration.ConfigureTestServices(
                    services => services.AddSingleton(emailServiceSubstitute)
                )
            );
        }
    }
}