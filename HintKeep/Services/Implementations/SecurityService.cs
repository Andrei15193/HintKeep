using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HintKeep.Services.Implementations
{
    public class SecurityService : ISecurityService
    {
        private readonly SecurityServiceConfig _securityServiceConfig;

        private readonly RandomNumberGenerator _rngProvider = RandomNumberGenerator.Create();

        public SecurityService(SecurityServiceConfig securityServiceConfig)
            => _securityServiceConfig = securityServiceConfig;

        public string GeneratePasswordSalt()
        {
            var passwordSaltBytes = new byte[_securityServiceConfig.SaltLength];
            _rngProvider.GetBytes(passwordSaltBytes);
            return Convert.ToBase64String(passwordSaltBytes);
        }

        public ConfirmationToken GenerateConfirmationToken()
        {
            var activationTokenBytes = new byte[_securityServiceConfig.ActivationTokenLength];
            _rngProvider.GetBytes(activationTokenBytes);
            var activationToken = string.Join("-", activationTokenBytes.Select(@byte => @byte.ToString("X2")));

            return new ConfirmationToken(activationToken, TimeSpan.FromMinutes(_securityServiceConfig.ActivationTokenExpirationMinutes));
        }

        public string ComputeHash(string value)
        {
            var hashAlgorithm = HashAlgorithm.Create(_securityServiceConfig.HashAlgorithm);
            var valueHash = hashAlgorithm.ComputeHash(Encoding.Default.GetBytes(value));
            return Convert.ToBase64String(valueHash);
        }

        public string ComputePasswordHash(string salt, string password)
        {
            var hashAlgorithm = HashAlgorithm.Create(_securityServiceConfig.HashAlgorithm);
            var passwordHashBytes = hashAlgorithm.ComputeHash(Encoding.Default.GetBytes(string.Format(_securityServiceConfig.PasswordFormat, CultureInfo.InvariantCulture, salt, password)));
            return Convert.ToBase64String(passwordHashBytes);
        }
    }
}