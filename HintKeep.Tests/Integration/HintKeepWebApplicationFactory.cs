using System;
using System.Linq;
using System.Net.Http;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Stubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace HintKeep.Tests.Integration
{
    public class HintKeepWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly string _userId;
        private readonly IEntityTables _entityTables;

        public HintKeepWebApplicationFactory()
        {
        }

        private HintKeepWebApplicationFactory(string authenticatedUserId, IEntityTables entityTables)
            => (_userId, _entityTables) = (authenticatedUserId, entityTables);

        public HintKeepWebApplicationFactory WithAuthentication(string userId)
        {
            if (_entityTables is null)
                throw new InvalidOperationException("In order to use authentication, an in-memory database has to be configured.");

            return new HintKeepWebApplicationFactory(userId, _entityTables);
        }

        public HintKeepWebApplicationFactory WithInMemoryDatabase()
            => WithInMemoryDatabase(out var _);

        public HintKeepWebApplicationFactory WithInMemoryDatabase(out IEntityTables entityTables)
        {
            var inMemoryEntityTables = new InMemoryEntityTables();
            var cloudTables = typeof(IEntityTables)
                .GetProperties()
                .Where(property => property.CanRead && property.PropertyType == typeof(CloudTable))
                .Select(property => property.GetValue(inMemoryEntityTables))
                .Cast<CloudTable>();
            foreach (var cloudTable in cloudTables)
                cloudTable.CreateIfNotExists();
            entityTables = inMemoryEntityTables;

            return new HintKeepWebApplicationFactory(_userId, entityTables);
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

            if (_entityTables is object)
                builder.ConfigureTestServices(services => services.AddSingleton(_entityTables));
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            if (!string.IsNullOrWhiteSpace(_userId))
            {
                const string sessionId = "#session-id";
                var entityTables = _entityTables ?? Services.GetService<IEntityTables>();
                entityTables.Users.Execute(TableOperation.Insert(new UserEntity
                {
                    EntityType = "UserEntity",
                    PartitionKey = _userId.ToEncodedKeyProperty(),
                    RowKey = "details".ToEncodedKeyProperty(),
                    Email = "test-user@domain.tld"
                }));
                entityTables.UserSessions.Execute(TableOperation.Insert(new UserSessionEntity
                {
                    EntityType = "UserSessionEntity",
                    PartitionKey = _userId.ToEncodedKeyProperty(),
                    RowKey = sessionId.ToEncodedKeyProperty(),
                    Expiration = DateTime.UtcNow.AddHours(1)
                }));

                var jsonWebTokenService = (IJsonWebTokenService)Services.GetService(typeof(IJsonWebTokenService));
                var jsonWebToken = jsonWebTokenService.GetJsonWebToken(_userId, sessionId);

                client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {jsonWebToken}");
            }
        }

        protected override IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder().ConfigureWebHostDefaults(config => config.UseStartup<Startup>().UseTestServer());
    }
}