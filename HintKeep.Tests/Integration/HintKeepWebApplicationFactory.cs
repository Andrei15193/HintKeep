using System.Linq;
using System.Net.Http;
using HintKeep.Services;
using HintKeep.Storage.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        private string _authenticatedEmail;

        public HintKeepWebApplicationFactory WithAuthentication(string email)
        {
            _authenticatedEmail = email;
            return this;
        }

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

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            if (!string.IsNullOrWhiteSpace(_authenticatedEmail))
            {
                var jsonWebTokenService = (IJsonWebTokenService)Services.GetService(typeof(IJsonWebTokenService));
                var jwt = jsonWebTokenService.GetJsonWebToken(_authenticatedEmail);
                client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {jwt}");
            }
        }

        protected override IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder().ConfigureWebHostDefaults(config => config.UseStartup<Startup>().UseTestServer());
    }
}