using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HintKeep.Services.Implementations.Configs
{
    public class JsonWebTokenServiceConfig
    {
        public JsonWebTokenServiceConfig(IConfigurationSection configurationSection)
        {
            SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationSection.GetValue<string>(nameof(SigningKey))));
            SigningAlgorithm = (string)typeof(SecurityAlgorithms)
                .GetField(
                    configurationSection.GetValue<string>(nameof(SigningAlgorithm)),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField
                )
                .GetValue(null);
            SigningCredentials = new SigningCredentials(SigningKey, SigningAlgorithm);
        }

        public SecurityKey SigningKey { get; }

        public string SigningAlgorithm { get; }

        public SigningCredentials SigningCredentials { get; }
    }
}