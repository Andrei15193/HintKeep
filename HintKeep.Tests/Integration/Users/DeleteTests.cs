using System;
using System.Net;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class DeleteTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public DeleteTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.DeleteAsync("/users");

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

            var response = await client.DeleteAsync("/users");

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

            var response = await client.DeleteAsync("/users?current=false");

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

            var response = await client.DeleteAsync("/users?current=true");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            var userEntity = Assert.Single(entityTables.Users.ExecuteQuery(new TableQuery<UserEntity>()));
            Assert.Equal("UserEntity", userEntity.EntityType);
            Assert.Equal("#user-id", userEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("details", userEntity.RowKey.FromEncodedKeyProperty());
            Assert.Equal("test-user@domain.tld", userEntity.Email);
        }

        [Fact]
        public async Task Delete_WithCurrentParameterAsFlag_ReturnsNoContent()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.DeleteAsync("/users?current");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            var userEntity = Assert.Single(entityTables.Users.ExecuteQuery(new TableQuery<UserEntity>()));
            Assert.Equal("UserEntity", userEntity.EntityType);
            Assert.Equal("#user-id", userEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("details", userEntity.RowKey.FromEncodedKeyProperty());
            Assert.Equal("test-user@domain.tld", userEntity.Email);
        }

        [Fact]
        public async Task Delete_WhenSessionHasExpired_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.UserSessions.Execute(TableOperation.Merge(new DynamicTableEntity
            {
                PartitionKey = "#user-id".ToEncodedKeyProperty(),
                RowKey = "#session-id".ToEncodedKeyProperty(),
                ETag = "*",
                Properties =
                {
                    { nameof(UserSessionEntity.Expiration), EntityProperty.GeneratePropertyForDateTimeOffset(DateTime.UtcNow.AddMinutes(-1)) }
                }
            }));

            var response = await client.DeleteAsync("/users?current");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenSessionDoesNotExist_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            foreach (var sessionEntity in entityTables.UserSessions.ExecuteQuery(new TableQuery()))
                entityTables.UserSessions.Execute(TableOperation.Delete(sessionEntity));

            var response = await client.DeleteAsync("/users?current");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenUserDoesNotExist_ReturnsUnauthorized()
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

            var response = await client.DeleteAsync("/users?current");

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
            entityTables.Users.Execute(TableOperation.Merge(new DynamicTableEntity
            {
                PartitionKey = "#user-id".ToEncodedKeyProperty(),
                RowKey = "details".ToEncodedKeyProperty(),
                ETag = "*",
                Properties =
                {
                    { nameof(UserEntity.IsDeleted), EntityProperty.GeneratePropertyForBool(true) }
                }
            }));

            var response = await client.DeleteAsync("/users?current");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}