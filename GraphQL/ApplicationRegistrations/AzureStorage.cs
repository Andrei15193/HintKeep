using Azure.Data.Tables;
using Azure.Identity;
using HintKeep.GraphQL.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HintKeep.GraphQL.ApplicationRegistrations;

public static class AzureStorage
{
    public const string ConnectionConfigurationName = "AzureWebJobsStorage";

    public static IServiceCollection AddAzureTableStorage(this IServiceCollection services)
        => services
            .AddSingleton<HintKeepTableStorage>()
            .AddSingleton(services =>
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                var configuration = services.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetValue<string>(ConnectionConfigurationName);
                var tableStorageUri = configuration.GetSection(ConnectionConfigurationName).GetValue<string>("tableServiceUri");

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    logger.LogInformation("Using connection string for TableServiceClient.");
                    return new TableServiceClient(connectionString);
                }
                else if (!string.IsNullOrWhiteSpace(tableStorageUri))
                {
                    logger.LogInformation("Using managed identity for TableServiceClient.");
                    return new TableServiceClient(new Uri(tableStorageUri), new DefaultAzureCredential());
                }
                else
                {
                    logger.LogCritical("Neither {connectionConfigurationName} (connection string) nor {connectionConfigurationName}__tableServiceUri (managed identity) have been configured for TableServiceClient.", ConnectionConfigurationName, ConnectionConfigurationName);
                    throw new InvalidOperationException($"Expected either {ConnectionConfigurationName} (connection string) or {ConnectionConfigurationName}__tableServiceUri (managed identity) to be configured.");
                }
            });
}
