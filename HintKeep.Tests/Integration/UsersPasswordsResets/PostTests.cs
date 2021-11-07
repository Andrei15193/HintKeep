using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Xunit;

namespace HintKeep.Tests.Integration.UsersPasswordsResets
{
    public class PostTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithInvalidEmail_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users/passwords/resets", new { email = "invalid-email-address" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidRequiredEmailAddress""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenActiveUserExists_ReturnsCreated()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithEmailService(out var emailService)
                .CreateClient();
            entityTables.Users.ExecuteBatch(
                new TableBatchOperation
                {
                    TableOperation.Insert(new UserEntity
                    {
                        PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                        RowKey = "details".ToEncodedKeyProperty(),
                        EntityType = "UserEntity",
                        IsActive = true,
                    })
                }
            );

            var response = await client.PostAsJsonAsync("/api/users/passwords/resets", new { email = "#TEST@domain.com" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/api/users/passwords/resets", UriKind.Relative), response.Headers.Location);

            var entities = entityTables.Users.ExecuteQuery(new TableQuery());
            var userPasswordResetTokenEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserPasswordResetTokenEntity");
            Assert.Equal("#test@domain.com".ToEncodedKeyProperty(), userPasswordResetTokenEntity.PartitionKey);
            Assert.NotEmpty(userPasswordResetTokenEntity.RowKey);
            Assert.Equal(2, userPasswordResetTokenEntity.Properties.Count);
            Assert.Equal("UserPasswordResetTokenEntity", userPasswordResetTokenEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            var expiration = userPasswordResetTokenEntity.Properties[nameof(UserPasswordResetTokenEntity.Expiration)].DateTimeOffsetValue;
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(55) < expiration);
            Assert.True(expiration < DateTimeOffset.UtcNow.AddMinutes(65));

            emailService.Verify(emailService => emailService.SendAsync("#TEST@domain.com", "HintKeep - Password Reset", It.Is<string>(body => body.Contains(userPasswordResetTokenEntity.RowKey))), Times.Once);
            emailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Post_WhenUserDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users/passwords/resets", new { email = "#TEST@domain.com" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenInactiveUserExists_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();
            entityTables.Users.ExecuteBatch(
                new TableBatchOperation
                {
                    TableOperation.Insert(new UserEntity
                    {
                        PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                        RowKey = "details".ToEncodedKeyProperty(),
                        EntityType = "UserEntity",
                        IsActive = false,
                    })
                }
            );

            var response = await client.PostAsJsonAsync("/api/users/passwords/resets", new { email = "#TEST@domain.com" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}