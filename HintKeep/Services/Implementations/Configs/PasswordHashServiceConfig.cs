using Microsoft.Extensions.Configuration;

namespace HintKeep.Services.Implementations.Configs
{
    public sealed class PasswordHashServiceConfig
    {
        public PasswordHashServiceConfig(IConfigurationSection configurationSection)
            => SaltPasswordFormat = configurationSection.GetValue<string>(nameof(SaltPasswordFormat));

        public string SaltPasswordFormat { get; }
    }
}