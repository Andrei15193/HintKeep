using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class PostTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithInvalidEmailHintAndPassword_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users", new { email = " ", hint = " ", password = " " });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""],""email"":[""validation.errors.invalidRequiredEmailAddress""],""password"":[""validation.errors.invalidRequiredPassword""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithInvalidEmailAddress_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users", new { email = "invalid-email-address", hint = "hint", password = "PA$$w0rd" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidRequiredEmailAddress""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithInvalidPassword_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users", new { email = "test@domain.com", hint = "hint", password = "password" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""password"":[""validation.errors.invalidRequiredPassword""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenUserDoesNotExist_ReturnsCreated()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithEmailService(out var emailService)
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users", new { email = "TEST@domain.com", hint = "#Test-Hint", password = "#Test-Password1" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/api/users/confirmations", UriKind.Relative), response.Headers.Location);

            var entities = entityTables.Users.ExecuteQuery(new TableQuery());
            var userEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserEntity");
            Assert.Equal("test@domain.com".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
            Assert.Equal(8, userEntity.Properties.Count);
            Assert.Equal("UserEntity", userEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.Id)].StringValue);
            Assert.Equal("TEST@domain.com", userEntity.Properties[nameof(UserEntity.Email)].StringValue);
            Assert.Equal("member", userEntity.Properties[nameof(UserEntity.Role)].StringValue);
            Assert.Equal("#Test-Hint", userEntity.Properties[nameof(UserEntity.Hint)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.PasswordHash)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.PasswordSalt)].StringValue);
            Assert.False(userEntity.Properties[nameof(UserEntity.IsActive)].BooleanValue);

            var userActivationTokenEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserActivationTokenEntity");
            Assert.Equal("test@domain.com".ToEncodedKeyProperty(), userActivationTokenEntity.PartitionKey);
            Assert.NotEmpty(userActivationTokenEntity.RowKey);
            Assert.Equal(2, userActivationTokenEntity.Properties.Count);
            Assert.Equal("UserActivationTokenEntity", userActivationTokenEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            var expiration = userActivationTokenEntity.Properties[nameof(UserActivationTokenEntity.Expiration)].DateTimeOffsetValue;
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(55) < expiration);
            Assert.True(expiration < DateTimeOffset.UtcNow.AddMinutes(65));

            emailService.Verify(emailService => emailService.SendAsync("TEST@domain.com", "Welcome to HintKeep!", It.Is<string>(body => body.Contains(userActivationTokenEntity.RowKey))), Times.Once);
            emailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Post_WhenUserEmailAlreadyExists_ReturnsConflict()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithEmailService(out var emailService)
                .CreateClient();
            entityTables.Users.Execute(TableOperation.Insert(new TableEntity { PartitionKey = "test@domain.com".ToEncodedKeyProperty(), RowKey = "details" }));

            var response = await client.PostAsJsonAsync("/api/users", new { email = "TEST@domain.com", hint = "#Test-Hint", password = "#Test-Password1" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            var entities = entityTables.Users.ExecuteQuery(new TableQuery());
            var userEntity = Assert.Single(entities);
            Assert.Equal("test@domain.com".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);

            emailService.VerifyNoOtherCalls();
        }
    }
}