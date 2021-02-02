
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.ViewModels.Users;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class PostAutenticationTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostAutenticationTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithEmptyObject_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PostAsJsonAsync("/users/authentications", new object());

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidEmail""],""password"":[""validation.errors.invalidPassword""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithInvalidEmailAndPassword_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PostAsJsonAsync("/users/authentications", new { email = "invalid-email", password = string.Empty });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidEmail""],""password"":[""validation.errors.invalidPassword""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithNonExistantUser_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.WithInMemoryDatabase().CreateClient();

            var response = await client.PostAsJsonAsync("/users/authentications", new { email = "eMail@DOMAIN.TLD", password = "test-PASSWORD-1" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithValidUser_ReturnsUserInfo()
        {
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();
            var cryptographicHashService = (ICryptographicHashService)_webApplicationFactory.Services.GetService(typeof(ICryptographicHashService));
            entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "email@domain.tld",
                RowKey = "EmailLogin",
                PasswordSalt = "test-salt",
                PasswordHash = cryptographicHashService.GetHash("test-salt" + "test-PASSWORD-1"),
                State = "Confirmed",
                UserId = Guid.NewGuid().ToString("N")
            }));
            
            var response = await client.PostAsJsonAsync("/users/authentications", new { email = "eMail@DOMAIN.TLD", password = "test-PASSWORD-1" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
            Assert.NotEmpty(userInfo.JsonWebToken);
        }

        [Fact]
        public async Task Post_WithUnconfirmedUser_ReturnsUnauthorized()
        {
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();
            var cryptographicHashService = (ICryptographicHashService)_webApplicationFactory.Services.GetService(typeof(ICryptographicHashService));
            entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "email@domain.tld",
                RowKey = "EmailLogin",
                PasswordSalt = "test-salt",
                PasswordHash = cryptographicHashService.GetHash("test-salt" + "test-PASSWORD-1"),
                State = "PendingConfirmation",
                UserId = Guid.NewGuid().ToString("N")
            }));

            var response = await client.PostAsJsonAsync("/users/authentications", new { email = "eMail@DOMAIN.TLD", password = "test-PASSWORD-1" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithInvalidPassword_ReturnsUnauthorized()
        {
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();
            var cryptographicHashService = (ICryptographicHashService)_webApplicationFactory.Services.GetService(typeof(ICryptographicHashService));
            entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "email@domain.tld",
                RowKey = "EmailLogin",
                PasswordSalt = "test-salt",
                PasswordHash = cryptographicHashService.GetHash("test-salt" + "test-PASSWORD-1"),
                State = "Confirmed"
            }));

            var response = await client.PostAsJsonAsync("/users/authentications", new { email = "eMail@DOMAIN.TLD", password = "test-PASSWORD-1-bad" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}