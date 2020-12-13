using System.Linq;
using System.Threading.Tasks;
using HintKeep.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Hosting;

namespace HintKeep
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>()).Build();

            _EnsureAzureTableStorage((IEntityTables)host.Services.GetService(typeof(IEntityTables)));

            host.Run();
        }

        private static void _EnsureAzureTableStorage(IEntityTables entityTables)
            => Task
                .WhenAll(
                    from property in typeof(IEntityTables).GetProperties()
                    where property.CanRead && property.PropertyType == typeof(CloudTable)
                    let cloudTable = (CloudTable)property.GetValue(entityTables)
                    select cloudTable.CreateIfNotExistsAsync()
                )
                .Wait();
    }
}