using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using NSubstitute;
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
                .WithSecurityService(out var securityService)
                .WithEmailService(out var emailService)
                .CreateClient();
            entityTables.Users.ExecuteBatch(
                new TableBatchOperation
                {
                    TableOperation.Insert(new UserEntity
                    {
                        PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                        RowKey = "details".ToEncodedKeyProperty(),
                        EntityType = "UserEntity",
                        IsActive = true,
                    })
                }
            );
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            securityService
                .GenerateConfirmationToken()
                .Returns(new ConfirmationToken("#confirmation-token", TimeSpan.FromHours(1)));

            var response = await client.PostAsJsonAsync("/api/users/passwords/resets", new { email = "#TEST@domain.com" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/api/users/passwords/resets", UriKind.Relative), response.Headers.Location);

            var entities = entityTables.Users.ExecuteQuery(new TableQuery());
            var userPasswordResetTokenEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserPasswordResetTokenEntity");
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userPasswordResetTokenEntity.PartitionKey);
            Assert.Equal("#confirmation-token".ToEncodedKeyProperty(), userPasswordResetTokenEntity.RowKey);
            Assert.Equal(2, userPasswordResetTokenEntity.Properties.Count);
            Assert.Equal("UserPasswordResetTokenEntity", userPasswordResetTokenEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            var expiration = userPasswordResetTokenEntity.Properties[nameof(UserPasswordResetTokenEntity.Expiration)].DateTimeOffsetValue;
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(55) < expiration);
            Assert.True(expiration < DateTimeOffset.UtcNow.AddMinutes(65));

            await emailService
                .Received()
                .SendAsync("#TEST@domain.com", "HintKeep - Password Reset", Arg.Is<string>(body => body.Contains("#confirmation-token")));
            Assert.Single(emailService.ReceivedCalls());
        }

        [Fact]
        public async Task Post_WhenUserDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithSecurityService(out var securityService)
                .CreateClient();
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");

            var response = await client.PostAsJsonAsync("/api/users/passwords/resets", new { email = "#TEST@domain.com" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenInactiveUserExists_ReturnsNotFound()
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
                        IsActive = false,
                    })
                }
            );
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");

            var response = await client.PostAsJsonAsync("/api/users/passwords/resets", new { email = "#TEST@domain.com" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}