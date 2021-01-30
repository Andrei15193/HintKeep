
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class DeleteAutenticationTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public DeleteAutenticationTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.DeleteAsync("/users/authentications");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WithoutCurrentParameter_ReturnsBadRequest()
        {
            var client = _webApplicationFactory.WithAuthentication(Guid.NewGuid().ToString("N")).CreateClient();

            var response = await client.DeleteAsync("/users/authentications");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WithCurrentParameterSetToFalse_ReturnsBadRequest()
        {
            var client = _webApplicationFactory.WithAuthentication(Guid.NewGuid().ToString("N")).CreateClient();

            var response = await client.DeleteAsync("/users/authentications?current=false");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WithCurrentParameterSetToTrue_ReturnsNoContent()
        {
            var client = _webApplicationFactory.WithAuthentication(Guid.NewGuid().ToString("N")).CreateClient();

            var response = await client.DeleteAsync("/users/authentications?current=true");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WithCurrentParameterAsFlag_ReturnsNoContent()
        {
            var client = _webApplicationFactory.WithAuthentication(Guid.NewGuid().ToString("N")).CreateClient();

            var response = await client.DeleteAsync("/users/authentications?current");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}