using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Accounts
{
    public class PostTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PostAsJsonAsync("/accounts", string.Empty);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithEmptyObject_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.WithAuthentication(Guid.NewGuid().ToString("N")).CreateClient();

            var response = await client.PostAsJsonAsync("/accounts", new object());

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""],""name"":[""validation.errors.invalidRequiredMediumText""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithInvalidNameHintAndNotes_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.WithAuthentication(Guid.NewGuid().ToString("N")).CreateClient();

            var response = await client.PostAsJsonAsync("/accounts", new { name = " ", hint = " ", notes = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""],""name"":[""validation.errors.invalidRequiredMediumText""],""notes"":[""validation.errors.invalidLongText""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithValidNameHintAndNotes_ReturnsCreated()
        {
            var userId = Guid.NewGuid().ToString("N");
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();

            var response = await client.PostAsJsonAsync("/accounts", new { name = "Test-Name", hint = "Test-Hint", notes = "Test-Notes", isPinned = true });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            var accountHintEntity = Assert.Single(entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(
                TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")
            )));

            Assert.Equal(new Uri($"/accounts/{accountHintEntity.AccountId}", UriKind.Relative), response.Headers.Location);
            entityTables.AssertAccounts(new Account
            {
                UserId = userId,
                Id = accountHintEntity.AccountId,
                Name = "Test-Name",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Hint = "Test-Hint",
                        StartDate = accountHintEntity.StartDate.Value
                    }
                }
            });
        }

        [Fact]
        public async Task Post_WithDuplicateName_ReturnsConflict()
        {
            var account = new Account();
            var client = _webApplicationFactory
                .WithAuthentication(account.UserId)
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();
            entityTables.AddAccounts(account);

            var response = await client.PostAsJsonAsync("/accounts", new { name = account.Name, hint = "Test-Hint", isPinned = true });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenDeletedAccountHasSameName_ReturnsConflict()
        {
            var account = new Account { IsDeleted = true };
            var client = _webApplicationFactory
                .WithAuthentication(account.UserId)
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();
            entityTables.AddAccounts(account);

            var response = await client.PostAsJsonAsync("/accounts", new { name = account.Name, hint = "Test-Hint", isPinned = true });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}