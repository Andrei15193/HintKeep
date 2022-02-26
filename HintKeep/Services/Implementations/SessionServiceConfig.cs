using Microsoft.IdentityModel.Tokens;

namespace HintKeep.Services.Implementations
{
    public record SessionServiceConfig(
        string ApplicationId,
        string Audience,
        SecurityKey SigningKey,
        string SingingAlgorithm
    );
}