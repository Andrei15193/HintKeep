using System.Linq;
using System.Threading.Tasks;
using HintKeep.Storage;
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
        {
            var cloudTables = typeof(IEntityTables)
                .GetProperties()
                .Where(property => property.CanRead && property.PropertyType == typeof(CloudTable))
                .Select(property => property.GetValue(entityTables))
                .Cast<CloudTable>();
            foreach (var cloudTable in cloudTables)
                cloudTable.CreateIfNotExists();
        }
    }
}