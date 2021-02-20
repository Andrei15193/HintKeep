using System.Globalization;
using HintKeep.Services.Implementations.Configs;

namespace HintKeep.Services.Implementations
{
    public class PasswordHashService : IPasswordHashService
    {
        private readonly PasswordHashServiceConfig _config;
        private readonly ICryptographicHashService _cryptographicHashService;

        public PasswordHashService(PasswordHashServiceConfig config, ICryptographicHashService cryptographicHashService)
            => (_config, _cryptographicHashService) = (config, cryptographicHashService);

        public string GetHash(string salt, string password)
            => _cryptographicHashService.GetHash(string.Format(CultureInfo.InvariantCulture, _config.SaltPasswordFormat, salt, password));
    }
}