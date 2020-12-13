using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HintKeep.Tests.Integration
{
    public class HintKeepWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseConfiguration(
                    new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile("appsettings.Development.json", optional: true)
                        .Build()
                );
        }

        protected override IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder().ConfigureWebHostDefaults(x => x.UseStartup<Startup>().UseTestServer());
    }
}