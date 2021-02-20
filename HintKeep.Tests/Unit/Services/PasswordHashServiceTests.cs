using HintKeep.Services;
using HintKeep.Services.Implementations;
using HintKeep.Services.Implementations.Configs;
using Moq;
using Xunit;

namespace HintKeep.Tests.Unit.Services
{
    public class PasswordHashServiceTests
    {
        [Fact]
        public void GetHashReturnsFormattedValue()
        {
            var cryptographicHashService = new Mock<ICryptographicHashService>();
            cryptographicHashService
                .Setup(service => service.GetHash("#salt-#password"))
                .Returns("#hashed-value")
                .Verifiable();
            var passwordHashService = new PasswordHashService(
                ServiceConfigFactory.Create<PasswordHashServiceConfig>(new { SaltPasswordFormat = "{0}-{1}" }),
                cryptographicHashService.Object
            );

            var result = passwordHashService.GetHash("#salt", "#password");
            Assert.Equal("#hashed-value", result);
        }
    }
}