using System.Linq;
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
    public class PutTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PutTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Put_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PutAsJsonAsync($"/accounts/account-id", string.Empty);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithEmptyObject_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.PutAsJsonAsync($"/accounts/account-id", new object());

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""],""name"":[""validation.errors.invalidRequiredMediumText""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithInvalidNameHintAndNotes_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.PutAsJsonAsync($"/accounts/account-id", new { name = " ", hint = " ", notes = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""],""name"":[""validation.errors.invalidRequiredMediumText""],""notes"":[""validation.errors.invalidLongText""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithMissingAccount_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.PutAsJsonAsync($"/accounts/account-id", new { name = "#Test-Name", hint = "#Test-Hint", isPinned = true });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithValidNameHintAndNotes_ReturnsNoContent()
        {
            var account = new Account { IsPinned = false };
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication(account.UserId)
                .CreateClient();
            entityTables.AddAccounts(account);

            var response = await client.PutAsJsonAsync($"/accounts/{account.Id}", new { name = "#Test-Name-Updated", hint = "#Test-Hint-Updated", notes = "#Test-Notes-Updated", isPinned = true });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            var latestAccountHintEntity = entityTables
                .Accounts
                .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(
                    TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")
                ))
                .Where(accountHintEntity => accountHintEntity.HintId != account.Hints.Single().Id)
                .Single();
            entityTables.AssertAccounts(new Account(account)
            {
                Name = "#Test-Name-Updated",
                Hints = new[]
                {
                    account.Hints.Single(),
                    new AccountHint
                    {
                        Id = latestAccountHintEntity.HintId,
                        Hint = "#Test-Hint-Updated",
                        DateAdded = latestAccountHintEntity.DateAdded.Value
                    }
                },
                Notes = "#Test-Notes-Updated",
                IsPinned = true
            });
        }

        [Fact]
        public async Task Put_WhenAccountIsDeleted_ReturnsNotFound()
        {
            var account = new Account { IsDeleted = true };
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication(account.UserId)
                .CreateClient();
            entityTables.AddAccounts(account);

            var response = await client.PutAsJsonAsync($"/accounts/{account.Id}", new { name = "#Test-Name-Updated", hint = "#Test-Hint-Updated", isPinned = true });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(account);
        }
    }
}