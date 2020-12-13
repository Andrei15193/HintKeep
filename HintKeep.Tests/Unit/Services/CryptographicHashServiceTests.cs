using HintKeep.Services.Implementations;
using HintKeep.Services.Implementations.Configs;
using Xunit;

namespace HintKeep.Tests.Unit.Services
{
    public class CryptographicHashServiceTests
    {
        [Theory]
        [InlineData("MD5", "83B3C112B82DCCA8376DA029E8101BCC")]
        [InlineData("SHA256", "5B1406FFFC9DE5537EB35A845C99521F26FBA0E772D58B42E09F4221B9E043AE")]
        [InlineData("SHA512", "0D3109314472E7D0FE12B0B4908AB74FC66636856A7C2A5BAEC35A1369E6D5EFB0441948BF30187129291702F05DD02375B1B377716A472D49B9B5B002D8866F")]
        public void GetHashReturnsHashedValue(string hashAlgorithm, string expectedHash)
        {
            var hashService = new CryptographicHashService(
                ServiceConfigFactory.Create<CryptographicHashServiceConfig>(new { HashAlgorithm = hashAlgorithm })
            );

            var actualHash = hashService.GetHash("test-value");
            Assert.Equal(expectedHash, actualHash);
        }
    }
}