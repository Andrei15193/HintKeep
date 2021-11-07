using Microsoft.IdentityModel.Tokens;

namespace HintKeep.Services.Implementations
{
    public class SessionServiceConfig
    {
        public string ApplicationId { get; init; }

        public string Audience { get; init; }

        public SecurityKey SigningKey { get; init; }

        public string SingingAlgorithm { get; init; }
    }
}