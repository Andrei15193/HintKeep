using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Xunit;

namespace HintKeep.Tests.Integration.UsersPasswords
{
    public class PostTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithInvalidTokenAndPassword_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users/passwords", new { token = "", password = "invalid-password" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""token"":[""validation.errors.invalidRequiredMediumText""],""password"":[""validation.errors.invalidRequiredPassword""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenTokenDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();

            entityTables.Users.Execute(TableOperation.Insert(new UserPasswordResetTokenEntity
            {
                PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                RowKey = "#token".ToEncodedKeyProperty(),
                EntityType = "UserPasswordResetTokenEntity",
                Expiration = DateTimeOffset.UtcNow.AddDays(-1)
            }));

            var response = await client.PostAsJsonAsync("/api/users/passwords", new { token = "#token", password = "passWORD$123" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenUserDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();

            entityTables.Users.Execute(TableOperation.Insert(new UserPasswordResetTokenEntity
            {
                PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                RowKey = "#token".ToEncodedKeyProperty(),
                EntityType = "UserPasswordResetTokenEntity",
                Expiration = DateTimeOffset.UtcNow.AddDays(1)
            }));

            var response = await client.PostAsJsonAsync("/api/users/passwords", new { token = "#token", password = "passWORD$123" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenValidTokenExist_ReturnsCreated()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithEmailService(out var emailService)
                .CreateClient();

            entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                    RowKey = "details",
                    EntityType = "UserEntity",
                    Id = "#id",
                    Email = "#TEST@domain.com",
                    Hint = "#hint",
                    PasswordHash = "#old-password-hash",
                    PasswordSalt = "#old-password-salt",
                    IsActive = true
                }),
                TableOperation.Insert(new UserPasswordResetTokenEntity
                {
                    PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                    RowKey = "#token".ToEncodedKeyProperty(),
                    EntityType = "UserPasswordResetTokenEntity",
                    Expiration = DateTimeOffset.UtcNow.AddDays(1)
                })
            });

            var response = await client.PostAsJsonAsync("/api/users/passwords", new { token = "#token", password = "passWORD$123" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/api/users/sessions", UriKind.Relative), response.Headers.Location);

            var userEntity = Assert.Single(entityTables.Users.ExecuteQuery(new TableQuery()));
            Assert.Equal("#test@domain.com".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
            Assert.Equal(7, userEntity.Properties.Count);
            Assert.Equal("UserEntity", userEntity.Properties[nameof(UserEntity.EntityType)].StringValue);
            Assert.Equal("#id", userEntity.Properties[nameof(UserEntity.Id)].StringValue);
            Assert.Equal("#TEST@domain.com", userEntity.Properties[nameof(UserEntity.Email)].StringValue);
            Assert.Equal("#hint", userEntity.Properties[nameof(UserEntity.Hint)].StringValue);
            Assert.NotEqual("#old-password-salt", userEntity.Properties[nameof(UserEntity.PasswordSalt)].StringValue);
            Assert.NotEqual("#old-password-hash", userEntity.Properties[nameof(UserEntity.PasswordHash)].StringValue);
            Assert.True(userEntity.Properties[nameof(UserEntity.IsActive)].BooleanValue);

            emailService.Verify(emailService => emailService.SendAsync("#TEST@domain.com", "HintKeep - Password Reset", It.IsAny<string>()), Times.Once);
            emailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Post_WhenInactiveUserExists_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();

            entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                    RowKey = "details",
                    EntityType = "UserEntity",
                    Id = "#id",
                    Email = "#TEST@domain.com",
                    Hint = "#hint",
                    PasswordHash = "#password-hash",
                    PasswordSalt = "#password-salt",
                    IsActive = false
                }),
                TableOperation.Insert(new UserPasswordResetTokenEntity
                {
                    PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                    RowKey = "#token".ToEncodedKeyProperty(),
                    EntityType = "UserPasswordResetTokenEntity",
                    Expiration = DateTimeOffset.UtcNow.AddDays(1)
                })
            });

            var response = await client.PostAsJsonAsync("/api/users/passwords", new { token = "#token", password = "passWORD$123" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            var userEntity = Assert.Single(entityTables.Users.ExecuteQuery(new TableQuery()));
            Assert.Equal("#test@domain.com".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
        }
    }
}