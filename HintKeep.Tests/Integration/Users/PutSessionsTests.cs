using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class PutSessionsTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PutSessionsTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Put_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PutAsJsonAsync("/users/sessions", string.Empty);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithoutCurrentParameter_ReturnsBadRequest()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.PutAsJsonAsync("/users/sessions", string.Empty);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithCurrentParameterSetToFalse_ReturnsBadRequest()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.PutAsJsonAsync("/users/sessions?current=false", string.Empty);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithCurrentParameterSetToTrue_ReturnsOk()
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
                    { nameof(UserSessionEntity.Expiration), EntityProperty.GeneratePropertyForDateTimeOffset(DateTime.UtcNow.AddMinutes(30)) }
                }
            }));

            var response = await client.PutAsJsonAsync("/users/sessions?current=true", string.Empty);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<UserSessionPupResult>();
            Assert.NotEmpty(result.JsonWebToken);
            var userSession = Assert.Single(entityTables.UserSessions.ExecuteQuery(new TableQuery<UserSessionEntity>()));
            Assert.Equal("UserSessionEntity", userSession.EntityType);
            Assert.Equal("#user-id", userSession.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("#session-id", userSession.RowKey.FromEncodedKeyProperty());
            Assert.True(DateTime.UtcNow.AddHours(1).AddMinutes(-1) <= userSession.Expiration && userSession.Expiration <= DateTime.UtcNow.AddHours(1).AddMinutes(1));
        }

        [Fact]
        public async Task Put_WithCurrentParameterAsFlag_ReturnsOk()
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
                    { nameof(UserSessionEntity.Expiration), EntityProperty.GeneratePropertyForDateTimeOffset(DateTime.UtcNow.AddMinutes(30)) }
                }
            }));

            var response = await client.PutAsJsonAsync("/users/sessions?current", string.Empty);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<UserSessionPupResult>();
            Assert.NotEmpty(result.JsonWebToken);
            var userSession = Assert.Single(entityTables.UserSessions.ExecuteQuery(new TableQuery<UserSessionEntity>()));
            Assert.Equal("UserSessionEntity", userSession.EntityType);
            Assert.Equal("#user-id", userSession.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("#session-id", userSession.RowKey.FromEncodedKeyProperty());
            Assert.True(DateTime.UtcNow.AddHours(1).AddMinutes(-1) <= userSession.Expiration && userSession.Expiration <= DateTime.UtcNow.AddHours(1).AddMinutes(1));
        }

        [Fact]
        public async Task Put_WhenSessionHasExpired_ReturnsUnauthorized()
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

            var response = await client.PutAsJsonAsync("/users/sessions?current", string.Empty);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WhenSessionIsDeleted_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            foreach (var sessionEntity in entityTables.UserSessions.ExecuteQuery(new TableQuery()))
                entityTables.UserSessions.Execute(TableOperation.Delete(sessionEntity));

            var response = await client.PutAsJsonAsync("/users/sessions?current", string.Empty);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WhenUserIsDeleted_ReturnsUnauthorized()
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

            var response = await client.PutAsJsonAsync("/users/sessions?current", string.Empty);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        private class UserSessionPupResult
        {
            public string JsonWebToken { get; set; }
        }
    }
}