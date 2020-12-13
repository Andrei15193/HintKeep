using HintKeep.Services.Implementations;
using Xunit;

namespace HintKeep.Tests.Unit.Services
{
    public class RngServiceTests
    {
        [Theory]
        [InlineData(9)]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        public void GenerateReturnsRandomStringOfConfiguredLength(int length)
        {
            var rngService = new RngService();

            var salt = rngService.Generate(length);

            Assert.Equal(length, salt.Length);
        }
    }
}