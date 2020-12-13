using Microsoft.Extensions.Configuration;

namespace HintKeep.Services.Implementations.Configs
{
    public sealed class SaltServiceConfig
    {
        public SaltServiceConfig(IConfigurationSection configurationSection)
            => SaltLength = configurationSection.GetValue<int>(nameof(SaltLength));

        public int SaltLength { get; }
    }
}