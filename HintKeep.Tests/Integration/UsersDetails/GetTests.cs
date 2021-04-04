using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace HintKeep.Tests.Integration.UsersDetails
{
    public class GetTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public GetTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Get_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/api/users/details");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAuthenticated_ReturnsNoContent()
        {
            var client = _webApplicationFactory
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.GetAsync("/api/users/details");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}