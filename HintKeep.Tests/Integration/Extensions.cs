using System;
using System.Linq;
using System.Threading.Tasks;
using HintKeep.Data;
using HintKeep.Services;
using HintKeep.Tests.Stubs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;

namespace HintKeep.Tests.Integration
{
    public static class Extensions
    {
        public static WebApplicationFactory<TEntryPoint> WithInMemoryDatabase<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, Action<IEntityTables> setupCallback = null)
                where TEntryPoint : class
            => webApplicationFactory.WithWebHostBuilder(
                builder => builder.ConfigureServices(
                    services =>
                    {
                        var inMemoryEntityTables = new InMemoryEntityTables();
                        Task
                            .WhenAll(
                                from property in typeof(InMemoryEntityTables).GetProperties()
                                where property.CanRead && typeof(CloudTable) == property.PropertyType
                                let cloudTable = (CloudTable)property.GetValue(inMemoryEntityTables)
                                select cloudTable.CreateAsync()
                            )
                            .Wait();

                        setupCallback?.Invoke(inMemoryEntityTables);
                        services.AddSingleton<IEntityTables>(inMemoryEntityTables);
                    }
                )
            );

        public static WebApplicationFactory<TEntryPoint> WithInMemoryEmailService<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, Action<InMemoryEmailService> setupCallback = null)
                where TEntryPoint : class
            => webApplicationFactory.WithWebHostBuilder(
                builder => builder.ConfigureServices(
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