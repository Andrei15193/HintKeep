using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using NSubstitute;
using Xunit;

namespace HintKeep.Tests.Integration.UsersSessions
{
    public class PostTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithInvalidEmailAndPassword_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users/sessions", new { email = "invalid-email-address", password = "" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidRequiredEmailAddress""],""password"":[""validation.errors.invalidRequiredMediumText""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenUserDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users/sessions", new { email = "#TEST@domain.com", password = "#test-password" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_WhenInactiveUserExists_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithSecurityService(out var securityService)
                .CreateClient();
            entityTables.Users.Execute(TableOperation.Insert(new UserEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "details".ToEncodedKeyProperty(),
                IsActive = false
            }));
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");

            var response = await client.PostAsJsonAsync("/api/users/sessions", new { email = "#TEST@domain.com", password = "#test-password" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_WhenActiveUserExistButPasswordsDoNotMatch_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithSecurityService(out var securityService)
                .CreateClient();
            securityService
                .ComputePasswordHash("#password-salt", "#test-password")
                .Returns("#password-hash-not-matching");
            entityTables.Users.Execute(TableOperation.Insert(new UserEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "details".ToEncodedKeyProperty(),
                PasswordSalt = "#password-salt",
                PasswordHash = "#password-hash",
                IsActive = true
            }));
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            securityService
                .ComputePasswordHash("#password-salt", "#test-password")
                .Returns("#password-hash-not-matching");

            var response = await client.PostAsJsonAsync("/api/users/sessions", new { email = "#TEST@domain.com", password = "#test-password" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""*"":[""errors.login.invalidCredentials""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenActiveUserExsitsWithMatchingPassword_ReturnsCreated()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithSecurityService(out var securityService)
                .CreateClient();
            entityTables.Users.Execute(TableOperation.Insert(new UserEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "details".ToEncodedKeyProperty(),
                Id = "#user-id",
                Role = "#user-role",
                PasswordSalt = "#password-salt",
                PasswordHash = "#password-hash",
                IsActive = true
            }));
            securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            securityService
                .ComputePasswordHash("#password-salt", "#test-password")
                .Returns("#password-hash");

            var response = await client.PostAsJsonAsync("/api/users/sessions", new { email = "#TEST@domain.com", password = "#test-password" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(new Uri("/api/users/sessions", UriKind.Relative), response.Headers.Location);
            Assert.NotEmpty(await response.Content.ReadAsStringAsync());

            var userEntity = Assert.Single(entityTables.Users.ExecuteQuery(new TableQuery<UserEntity>()));
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details".ToEncodedKeyProperty(), userEntity.RowKey);
            Assert.NotNull(userEntity.LastLoginTime);
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(-1) <= userEntity.LastLoginTime && userEntity.LastLoginTime <= DateTimeOffset.UtcNow.AddMinutes(1));
        }
    }
}