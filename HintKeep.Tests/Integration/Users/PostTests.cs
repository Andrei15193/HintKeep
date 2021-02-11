using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Stubs;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class PostTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithEmptyObject_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PostAsJsonAsync("/users", new object());

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidEmail""],""password"":[""validation.errors.invalidPassword""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithInvalidEmailAndPassword_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PostAsJsonAsync("/users", new { email = "invalid-email", password = "invalid-password" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidEmail""],""password"":[""validation.errors.invalidPassword""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithValidEmailAndPassword_ReturnsCreated()
        {
            var emailService = default(InMemoryEmailService);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithInMemoryEmailService(actualEmailService => emailService = actualEmailService)
                .CreateClient();

            var response = await client.PostAsJsonAsync("/users", new { email = "#eMail@DOMAIN.TLD", password = "#test-PASSWORD-1" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/users/confirmations", UriKind.Relative), response.Headers.Location);

            var emailLoginEntity = (EmailLoginEntity)entityTables.Logins.Execute(TableOperation.Retrieve<EmailLoginEntity>("#email@domain.tld".ToEncodedKeyProperty(), "EmailLogin".ToEncodedKeyProperty())).Result;
            Assert.Equal("EmailLoginEntity", emailLoginEntity.EntityType);
            Assert.Equal("#email@domain.tld", emailLoginEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("EmailLogin", emailLoginEntity.RowKey.FromEncodedKeyProperty());
            Assert.Equal("PendingConfirmation", emailLoginEntity.State);
            Assert.Equal(128, emailLoginEntity.PasswordHash.Length);
            Assert.Equal(10, emailLoginEntity.PasswordSalt.Length);

            var tokenEntity = (EmailLoginTokenEntity)entityTables.Logins.Execute(TableOperation.Retrieve<EmailLoginTokenEntity>("#email@domain.tld".ToEncodedKeyProperty(), "EmailLogin-confirmationToken".ToEncodedKeyProperty())).Result;
            Assert.Equal("EmailLoginTokenEntity", tokenEntity.EntityType);
            Assert.Equal("#email@domain.tld", tokenEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("EmailLogin-confirmationToken", tokenEntity.RowKey.FromEncodedKeyProperty());
            Assert.Equal(12, tokenEntity.Token.Length);
            Assert.True(DateTime.UtcNow.AddMinutes(-1) <= tokenEntity.Created && tokenEntity.Created <= DateTime.UtcNow.AddMinutes(1));

            var userEntity = (UserEntity)entityTables.Users.Execute(TableOperation.Retrieve<UserEntity>(emailLoginEntity.UserId.ToEncodedKeyProperty(), "details".ToEncodedKeyProperty())).Result;
            Assert.Equal(emailLoginEntity.UserId, userEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("details", userEntity.RowKey.FromEncodedKeyProperty());
            Assert.Equal("#eMail@DOMAIN.TLD", userEntity.Email);

            var confirmationEmail = Assert.Single(emailService.SentEmailMessages);
            Assert.Equal("Welcome to HintKeep!", confirmationEmail.Title);
            Assert.Equal("#eMail@DOMAIN.TLD", confirmationEmail.To);
            Assert.Contains(tokenEntity.Token, confirmationEmail.Content);
        }

        [Fact]
        public async Task Post_WithExistingEmailAddress_ReturnsConflict()
        {
            var emailService = default(InMemoryEmailService);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithInMemoryEmailService(actualEmailService => emailService = actualEmailService)
                .CreateClient();

            entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "#email@domain.tld".ToEncodedKeyProperty(),
                RowKey = "EmailLogin".ToEncodedKeyProperty(),
                PasswordSalt = "#test-salt",
                PasswordHash = "#test-hash",
                State = nameof(EmailLoginEntityState.PendingConfirmation),
                UserId = "#user-id"
            }));

            var response = await client.PostAsJsonAsync("/users", new { email = "#eMail@DOMAIN.TLD", password = "#test-PASSWORD-1" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Empty(emailService.SentEmailMessages);
        }
    }
}