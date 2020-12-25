using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Stubs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class BasicTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly WebApplicationFactory<Startup> _webApplicationFactory;

        public BasicTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task PostWithEmptyObjectReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PostAsJsonAsync("/users", new object());

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidEmail""],""password"":[""validation.errors.invalidPassword""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task PostWithInvalidEmailAndPasswordReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PostAsJsonAsync("/users", new { email = "invalid-email", password = "invalid-password" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidEmail""],""password"":[""validation.errors.invalidPassword""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task PostWithValidEmailAndPasswordReturnsCreated()
        {
            var entityTables = default(IEntityTables);
            var emailService = default(InMemoryEmailService);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .WithInMemoryEmailService(actualEmailService => emailService = actualEmailService)
                .CreateClient();

            var response = await client.PostAsJsonAsync("/users", new { email = "eMail@DOMAIN.TLD", password = "test-PASSWORD-1" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/users/confirmations", UriKind.Relative), response.Headers.Location);

            var userEntity = (UserEntity)entityTables.Users.Execute(TableOperation.Retrieve<UserEntity>("email@domain.tld", "user")).Result;
            Assert.Equal("email@domain.tld", userEntity.PartitionKey);
            Assert.Equal("user", userEntity.RowKey);
            Assert.Equal("eMail@DOMAIN.TLD", userEntity.Email);
            Assert.Equal(UserState.PendingConfirmation, (UserState)userEntity.State);
            Assert.Equal(128, userEntity.PasswordHash.Length);
            Assert.Equal(10, userEntity.PasswordSalt.Length);

            var tokenEntityQuery = new TableQuery<TokenEntity>()
                .Where(TableQuery.GenerateFilterCondition(nameof(TokenEntity.RowKey), QueryComparisons.NotEqual, string.Empty))
                .Take(1);
            var tokenEntity = Assert.Single(entityTables.Users.ExecuteQuery<TokenEntity>(tokenEntityQuery, null));
            Assert.Equal("email@domain.tld", tokenEntity.PartitionKey);
            Assert.Equal("confirmation_tokens-" + tokenEntity.Token, tokenEntity.RowKey);
            Assert.Equal(12, tokenEntity.Token.Length);
            Assert.Equal(TokenIntent.ConfirmUserRegistration, (TokenIntent)tokenEntity.Intent);

            var confirmationEmail = Assert.Single(emailService.SentEmailMessages);
            Assert.Equal("Welcome to HintKeep!", confirmationEmail.Title);
            Assert.Equal("eMail@DOMAIN.TLD", confirmationEmail.To);
            Assert.Contains(tokenEntity.Token, confirmationEmail.Content);
            Assert.True(DateTime.UtcNow.AddMinutes(-1) <= tokenEntity.Created && tokenEntity.Created <= DateTime.UtcNow.AddMinutes(1));
        }
    }
}