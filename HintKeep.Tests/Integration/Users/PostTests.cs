using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using NSubstitute;
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
                .WithSecurityService(out var securityService)
                .WithEmailService(out var emailService)
                .CreateClient();
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            securityService
                .GeneratePasswordSalt()
                .Returns("#password-salt");
            securityService
                .ComputePasswordHash("#password-salt", "#Test-Password1")
                .Returns("#password-hash");
            securityService
                .GenerateConfirmationToken()
                .Returns(new ConfirmationToken("#confirmation-token", TimeSpan.FromHours(1)));

            var response = await client.PostAsJsonAsync("/api/users", new { email = "#TEST@domain.com", hint = "#Test-Hint", password = "#Test-Password1" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/api/users/confirmations", UriKind.Relative), response.Headers.Location);

            var entities = entityTables.Users.ExecuteQuery(new TableQuery());
            var userEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserEntity");
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
            Assert.Equal(7, userEntity.Properties.Count);
            Assert.Equal("UserEntity", userEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.Id)].StringValue);
            Assert.Equal("member", userEntity.Properties[nameof(UserEntity.Role)].StringValue);
            Assert.Equal("#Test-Hint", userEntity.Properties[nameof(UserEntity.Hint)].StringValue);
            Assert.Equal("#password-hash", userEntity.Properties[nameof(UserEntity.PasswordHash)].StringValue);
            Assert.Equal("#password-salt", userEntity.Properties[nameof(UserEntity.PasswordSalt)].StringValue);
            Assert.False(userEntity.Properties[nameof(UserEntity.IsActive)].BooleanValue);

            var userActivationTokenEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserActivationTokenEntity");
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userActivationTokenEntity.PartitionKey);
            Assert.Equal("#confirmation-token".ToEncodedKeyProperty(), userActivationTokenEntity.RowKey);
            Assert.Equal(2, userActivationTokenEntity.Properties.Count);
            Assert.Equal("UserActivationTokenEntity", userActivationTokenEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            var expiration = userActivationTokenEntity.Properties[nameof(UserActivationTokenEntity.Expiration)].DateTimeOffsetValue;
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(55) < expiration);
            Assert.True(expiration < DateTimeOffset.UtcNow.AddMinutes(65));

            await emailService
                .Received()
                .SendAsync("#TEST@domain.com", "Welcome to HintKeep!", Arg.Is<string>(body => body.Contains("#confirmation-token")));
            Assert.Single(emailService.ReceivedCalls());
        }

        [Fact]
        public async Task Post_WhenUserEmailAlreadyExists_ReturnsConflict()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithSecurityService(out var securityService)
                .WithEmailService(out var emailService)
                .CreateClient();
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            securityService
                .GenerateConfirmationToken()
                .Returns(new ConfirmationToken("#confirmation-token", TimeSpan.FromHours(1)));
            entityTables.Users.Execute(TableOperation.Insert(new TableEntity { PartitionKey = "#email-hash".ToEncodedKeyProperty(), RowKey = "details" }));

            var response = await client.PostAsJsonAsync("/api/users", new { email = "#TEST@domain.com", hint = "#Test-Hint", password = "#Test-Password1" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            var entities = entityTables.Users.ExecuteQuery(new TableQuery());
            var userEntity = Assert.Single(entities);
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);

            Assert.Empty(emailService.ReceivedCalls());
        }
    }
}