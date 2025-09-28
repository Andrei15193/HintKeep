using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.GraphQL.AppSetup.Registrations;

public static class JsonWebTokens
{
    public static IServiceCollection AddJsonWebTokens(this IServiceCollection services, string serviceKey, string signingKeyConfigurationName)
        => services
            .AddKeyedSingleton<SecurityKey>(serviceKey, (services, serviceKey) =>
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                var configuration = services.GetRequiredService<IConfiguration>();
                var signingKey = configuration.GetValue<string>(signingKeyConfigurationName);

                if (string.IsNullOrWhiteSpace(signingKey))
                {
                    logger.LogCritical("{signingKeyConfigurationName} has not been configured.", signingKeyConfigurationName);
                    throw new InvalidOperationException($"Expected {signingKeyConfigurationName} to be configured.");
                }

                return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            })
            .AddKeyedSingleton<SigningCredentials>(serviceKey, (services, serviceKey) =>
            {
                return new SigningCredentials(services.GetRequiredKeyedService<SecurityKey>(serviceKey), SecurityAlgorithms.HmacSha256)
                {
                    CryptoProviderFactory = new CryptoProviderFactory
                    {
                        CacheSignatureProviders = false
                    }
                };
            })
            .AddKeyedSingleton<TokenValidationParameters>(serviceKey, (services, serviceKey) => new TokenValidationParameters
            {
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                IgnoreTrailingSlashWhenValidatingAudience = false,
                IncludeTokenOnFailedValidation = false,
                ValidateIssuer = true,
                ValidIssuer = "hintkeep",

                RequireAudience = true,
                ValidateAudience = true,
                ValidAudience = "graphql",

                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = services.GetRequiredKeyedService<SecurityKey>(serviceKey)
            })
            .AddSingleton<JwtSecurityTokenHandler>(services =>
            {
                var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

                jwtSecurityTokenHandler.InboundClaimTypeMap.Clear();
                jwtSecurityTokenHandler.OutboundClaimTypeMap.Clear();

                return jwtSecurityTokenHandler;
            })
            .AddKeyedSingleton<JwtSecurityTokenHandler>(serviceKey, (services, _) => services.GetRequiredService<JwtSecurityTokenHandler>());
}