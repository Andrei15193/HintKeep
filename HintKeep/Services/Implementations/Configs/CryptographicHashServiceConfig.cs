using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace HintKeep.Services.Implementations.Configs
{
    public sealed class CryptographicHashServiceConfig : IDisposable
    {
        public CryptographicHashServiceConfig(IConfigurationSection configurationSection)
            => HashAlgorithm = HashAlgorithm.Create(configurationSection.GetValue<string>(nameof(HashAlgorithm)));

        public HashAlgorithm HashAlgorithm { get; }

        public void Dispose()
            => HashAlgorithm.Dispose();
    }
}