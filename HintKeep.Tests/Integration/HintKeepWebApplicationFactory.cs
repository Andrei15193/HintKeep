using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace HintKeep.Tests.Integration
{
    public class HintKeepWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder.UseConfiguration(
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build()
            );

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            client.DefaultRequestHeaders.Add(HeaderNames.Authorization, TestAuthenticationExtensions.TestAuthenticationScheme + " invalid-token");
        }

        protected override IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder().ConfigureWebHostDefaults(config => config.UseStartup<Startup>().UseTestServer());
    }
}