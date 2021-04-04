using System.Linq;
using HintKeep.Storage;
using HintKeep.Tests.Stubs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.Tests.Integration
{
    public static class TestDatabaseExtensions
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
            return webApplicationFactory.WithWebHostBuilder(
                configuration => configuration.ConfigureTestServices(
                    services => services.AddSingleton<IEntityTables>(inMemoryEntityTables)
                )
            );
        }
    }
}