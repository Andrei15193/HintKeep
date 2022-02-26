using System.Linq;
using HintKeep;
using HintKeep.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Hosting;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
    .Build();

foreach (var cloudTable in from property in typeof(IEntityTables).GetProperties()
                           let entityTables = (IEntityTables)host.Services.GetService(typeof(IEntityTables))
                           where property.CanRead && property.PropertyType == typeof(CloudTable)
                           select (CloudTable)property.GetValue(entityTables))
    cloudTable.CreateIfNotExists();

host.Run();