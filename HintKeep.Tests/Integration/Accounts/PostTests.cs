using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
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
        public async Task Post_WithInvalidNameAndPassword_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.WithAuthentication(Guid.NewGuid().ToString("N")).CreateClient();

            var response = await client.PostAsJsonAsync("/accounts", new { name = " ", hint = " " });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""],""name"":[""validation.errors.invalidRequiredMediumText""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithValidNameAndHint_ReturnsCreated()
        {
            var userId = Guid.NewGuid().ToString("N");
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();

            var response = await client.PostAsJsonAsync("/accounts", new { name = "Test-Account", hint = "Test-Hint", isPinned = true });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            var indexedEntity = (IndexEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(userId, "name-test-account")).Result;
            Assert.Equal(userId, indexedEntity.PartitionKey);
            Assert.Equal("name-test-account", indexedEntity.RowKey);
            Assert.NotEmpty(indexedEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(userId, $"id-{indexedEntity.IndexedEntityId}")).Result;
            Assert.Equal(userId, accountEntity.PartitionKey);
            Assert.Equal($"id-{indexedEntity.IndexedEntityId}", accountEntity.RowKey);
            Assert.Equal(indexedEntity.IndexedEntityId, accountEntity.Id);
            Assert.Equal("Test-Account", accountEntity.Name);
            Assert.Equal("Test-Hint", accountEntity.Hint);
            Assert.True(accountEntity.IsPinned);

            var accountHintEntity = Assert.Single(entityTables.Accounts.ExecuteQuery(
                new TableQuery<AccountHintEntity>()
                    .Where(TableQuery.GenerateFilterCondition(nameof(AccountHintEntity.AccountId), QueryComparisons.NotEqual, string.Empty))
            ));
            Assert.Equal(userId, accountHintEntity.PartitionKey);
            Assert.Equal($"id-{indexedEntity.IndexedEntityId}-hintDate-{accountHintEntity.StartDate:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", accountHintEntity.RowKey);
            Assert.NotNull(accountHintEntity.StartDate);
            Assert.Equal(accountEntity.Id, accountHintEntity.AccountId);
            Assert.Equal("Test-Hint", accountHintEntity.Hint);

            Assert.Equal(new Uri($"/accounts/{indexedEntity.IndexedEntityId}", UriKind.Relative), response.Headers.Location);
        }

        [Fact]
        public async Task Post_WithDuplicateName_ReturnsConflict()
        {
            var userId = Guid.NewGuid().ToString("N");
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();
            entityTables.Accounts.Execute(TableOperation.Insert(new IndexEntity
            {
                PartitionKey = userId,
                RowKey = "name-test-account"
            }));

            var response = await client.PostAsJsonAsync("/accounts", new { name = "Test-Account", hint = "Test-Hint", isPinned = true });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}