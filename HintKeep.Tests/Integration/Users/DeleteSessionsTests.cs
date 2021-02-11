using System.Net;
using System.Threading.Tasks;
using HintKeep.Storage;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class DeleteSessionsTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public DeleteSessionsTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.DeleteAsync("/users/sessions");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WithoutCurrentParameter_ReturnsBadRequest()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.DeleteAsync("/users/sessions");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WithCurrentParameterSetToFalse_ReturnsBadRequest()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.DeleteAsync("/users/sessions?current=false");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WithCurrentParameterSetToTrue_ReturnsNoContent()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.DeleteAsync("/users/sessions?current=true");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Empty(entityTables.UserSessions.ExecuteQuery(new TableQuery()));
        }

        [Fact]
        public async Task Delete_WithCurrentParameterAsFlag_ReturnsNoContent()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.DeleteAsync("/users/sessions?current");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Empty(entityTables.UserSessions.ExecuteQuery(new TableQuery()));
        }

        [Fact]
        public async Task Delete_WhenSessionIsDeleted_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            foreach (var sessionEntity in entityTables.UserSessions.ExecuteQuery(new TableQuery()))
                entityTables.UserSessions.Execute(TableOperation.Delete(sessionEntity));

            var response = await client.DeleteAsync("/users/sessions?current");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenUserIsDeleted_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.Users.Execute(TableOperation.Delete(new TableEntity
            {
                PartitionKey = "#user-id".ToEncodedKeyProperty(),
                RowKey = "details".ToEncodedKeyProperty(),
                ETag = "*"
            }));

            var response = await client.DeleteAsync("/users/sessions?current");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}