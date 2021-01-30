using System;
using System.Net.Http;
using HintKeep.Services;
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
        private readonly string _authenticatedUserId;

        public HintKeepWebApplicationFactory()
        {
        }

        private HintKeepWebApplicationFactory(string authenticatedUserId)
            => _authenticatedUserId = authenticatedUserId;

        public HintKeepWebApplicationFactory WithAuthentication(string userId)
            => new HintKeepWebApplicationFactory(userId);

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
            if (!string.IsNullOrWhiteSpace(_authenticatedUserId))
            {
                var jsonWebTokenService = (IJsonWebTokenService)Services.GetService(typeof(IJsonWebTokenService));
                var jwt = jsonWebTokenService.GetJsonWebToken(_authenticatedUserId);
                client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {jwt}");
            }
        }

        protected override IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder().ConfigureWebHostDefaults(config => config.UseStartup<Startup>().UseTestServer());
    }
}