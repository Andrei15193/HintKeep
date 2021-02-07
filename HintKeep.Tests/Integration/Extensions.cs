using System;
using System.Linq;
using HintKeep.Storage;
using HintKeep.Services;
using HintKeep.Tests.Stubs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;

namespace HintKeep.Tests.Integration
{
    public static class Extensions
    {
        public static WebApplicationFactory<TEntryPoint> WithInMemoryDatabase<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory)
            where TEntryPoint : class
            => webApplicationFactory.WithInMemoryDatabase(out var _);

        public static WebApplicationFactory<TEntryPoint> WithInMemoryDatabase<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, out IEntityTables entityTables)
            where TEntryPoint : class
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

            return webApplicationFactory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services.AddSingleton<IEntityTables>(inMemoryEntityTables)));
        }

        public static WebApplicationFactory<TEntryPoint> WithInMemoryEmailService<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, Action<InMemoryEmailService> setupCallback = null)
                where TEntryPoint : class
            => webApplicationFactory.WithWebHostBuilder(
                builder => builder.ConfigureTestServices(
                    services =>
                    {
                        var inMemoryEmailService = new InMemoryEmailService();
                        setupCallback?.Invoke(inMemoryEmailService);
                        services.AddSingleton<IEmailService>(inMemoryEmailService);
                    }
                )
            );
    }
}