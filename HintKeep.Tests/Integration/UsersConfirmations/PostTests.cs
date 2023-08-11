using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using NSubstitute;
using Xunit;

namespace HintKeep.Tests.Integration.UsersConfirmations
{
    public class PostTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithInvalidToken_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users/confirmations", new { token = " " });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""token"":[""validation.errors.invalidRequiredMediumText""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenValidTokenExist_ReturnsCreated()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithSecurityService(out var securityService)
                .CreateClient();
            entityTables.Users.ExecuteBatch(
                new TableBatchOperation
                {
                    TableOperation.Insert(new UserEntity
                    {
                        PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                        RowKey = "details".ToEncodedKeyProperty(),
                        EntityType = "UserEntity",
                        Hint = "#test-hint",
                        Id = "#user-id",
                        IsActive = false,
                        PasswordSalt = "#password-salt",
                        PasswordHash = "#password-hash"
                    }),
                    TableOperation.Insert(new UserActivationTokenEntity
                    {
                        PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                        RowKey = "#test-token".ToEncodedKeyProperty(),
                        EntityType = "UserActivationTokenEntity",
                        Expiration = DateTimeOffset.UtcNow.AddDays(1)
                    })
                }
            );
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");

            var response = await client.PostAsJsonAsync("/api/users/confirmations", new { token = "#test-token" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/api/users/sessions", UriKind.Relative), response.Headers.Location);

            var entities = entityTables.Users.ExecuteQuery(new TableQuery());
            var userEntity = Assert.Single(entities);
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
            Assert.Equal(6, userEntity.Properties.Count);
            Assert.Equal("UserEntity", userEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.Id)].StringValue);
            Assert.Equal("#test-hint", userEntity.Properties[nameof(UserEntity.Hint)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.PasswordHash)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.PasswordSalt)].StringValue);
            Assert.True(userEntity.Properties[nameof(UserEntity.IsActive)].BooleanValue);
        }

        [Fact]
        public async Task Post_WhenTokenDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users/confirmations", new { token = "does not exist" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenExpiredTokenExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithSecurityService(out var securityService)
                .CreateClient();
            entityTables.Users.Execute(TableOperation.Insert(new UserActivationTokenEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "#token".ToEncodedKeyProperty(),
                EntityType = "UserActivationTokenEntity",
                Expiration = DateTimeOffset.UtcNow.AddDays(-1)
            }));
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");

            var response = await client.PostAsJsonAsync("/api/users/confirmations", new { token = "#token" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Empty(entityTables.Users.ExecuteQuery(new TableQuery()));
        }
    }
}