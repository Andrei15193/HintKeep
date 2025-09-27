using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HintKeep.GraphQL.ApplicationRegistrations;

public static class HashAlgorithms
{
    public static IServiceCollection AddHmacSha256HashAlgorithm(this IServiceCollection services, string serviceKey, string hashKeyConfigurationName)
        => services
            .AddKeyedScoped<HashAlgorithm>(serviceKey, (services, serviceKey) =>
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                var configuration = services.GetRequiredService<IConfiguration>();
                var hashKey = configuration.GetValue<string>(hashKeyConfigurationName);

                if (string.IsNullOrWhiteSpace(hashKey))
                {
                    logger.LogCritical("{hashKeyConfigurationName} has not been configured.", hashKeyConfigurationName);
                    throw new InvalidOperationException($"Expected {hashKeyConfigurationName} to be configured.");
                }

                return new HMACSHA256(Encoding.UTF8.GetBytes(hashKey));
            });
}