using HintKeep.Services.Implementations;
using HintKeep.Services.Implementations.Configs;
using Xunit;

namespace HintKeep.Tests.Unit.Services
{
    public class SaltServiceTests
    {
        [Theory]
        [InlineData(9)]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        public void GetSaltReturnsRandomStringOfConfiguredLength(int length)
        {
            var saltService = new SaltService(ServiceConfigFactory.Create<SaltServiceConfig>(new { SaltLength = length }), new RngService());

            var salt = saltService.GetSalt();

            Assert.Equal(length, salt.Length);
        }
    }
}