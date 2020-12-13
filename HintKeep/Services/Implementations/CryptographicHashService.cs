using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HintKeep.Services.Implementations.Configs;

namespace HintKeep.Services.Implementations
{
    public class CryptographicHashService : ICryptographicHashService
    {
        private readonly CryptographicHashServiceConfig _config;

        public CryptographicHashService(CryptographicHashServiceConfig config)
            => _config = config;

        public string GetHash(string value)
        {
            var hash = _config.HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            return hash.Aggregate(new StringBuilder(hash.Length * 2), (result, @byte) => result.AppendFormat("{0:X2}", @byte)).ToString();
        }
    }
}