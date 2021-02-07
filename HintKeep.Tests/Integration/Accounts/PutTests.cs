using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
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
            var accountId = Guid.NewGuid().ToString("N");
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PutAsJsonAsync($"/accounts/{accountId}", string.Empty);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithEmptyObject_ReturnsUnprocessableEntity()
        {
            var accountId = Guid.NewGuid().ToString("N");
            var client = _webApplicationFactory.WithAuthentication(Guid.NewGuid().ToString("N")).CreateClient();

            var response = await client.PutAsJsonAsync($"/accounts/{accountId}", new object());

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""],""name"":[""validation.errors.invalidRequiredMediumText""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithInvalidNameHintAndNotes_ReturnsUnprocessableEntity()
        {
            var accountId = Guid.NewGuid().ToString("N");
            var client = _webApplicationFactory.WithAuthentication(Guid.NewGuid().ToString("N")).CreateClient();

            var response = await client.PutAsJsonAsync($"/accounts/{accountId}", new { name = " ", hint = " ", notes = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""],""name"":[""validation.errors.invalidRequiredMediumText""],""notes"":[""validation.errors.invalidLongText""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithMissingAccount_ReturnsNotFound()
        {
            var accountId = Guid.NewGuid().ToString("N");
            var client = _webApplicationFactory
                .WithAuthentication(Guid.NewGuid().ToString("N"))
                .WithInMemoryDatabase()
                .CreateClient();

            var response = await client.PutAsJsonAsync($"/accounts/{accountId}", new { name = "test-name", hint = "test-hint", isPinned = true });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithValidNameHintAndNotes_ReturnsNoContent()
        {
            var userId = Guid.NewGuid().ToString("N");
            var accountId = Guid.NewGuid().ToString("N");
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();
            var yesterday = DateTime.UtcNow;
            entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = $"name-test-name",
                    IndexedEntityId = accountId
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{accountId}",
                    Id = accountId,
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = false,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{accountId}-hintDate-{yesterday:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    AccountId = accountId,
                    Hint = "test-hint",
                    StartDate = yesterday
                })
            });

            var response = await client.PutAsJsonAsync($"/accounts/{accountId}", new { name = "test-name-updated", hint = "test-hint-updated", notes = "test-notes-updated", isPinned = true });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            Assert.Equal(4, entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexEntity = (IndexEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(userId, "name-test-name-updated")).Result;
            Assert.Equal("IndexEntity", indexEntity.EntityType);
            Assert.Equal(userId, indexEntity.PartitionKey);
            Assert.Equal("name-test-name-updated", indexEntity.RowKey);
            Assert.Equal(accountId, indexEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(userId, $"id-{accountId}")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal(userId, accountEntity.PartitionKey);
            Assert.Equal($"id-{accountId}", accountEntity.RowKey);
            Assert.Equal("test-name-updated", accountEntity.Name);
            Assert.Equal("test-hint-updated", accountEntity.Hint);
            Assert.Equal("test-notes-updated", accountEntity.Notes);
            Assert.True(accountEntity.IsPinned);
            Assert.False(accountEntity.IsDeleted);

            var olderAccountHintEntity = (AccountHintEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>(userId, $"id-{accountId}-hintDate-{yesterday:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}")).Result;
            Assert.Equal("AccountHintEntity", olderAccountHintEntity.EntityType);
            Assert.Equal(userId, olderAccountHintEntity.PartitionKey);
            Assert.Equal($"id-{accountId}-hintDate-{yesterday:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", olderAccountHintEntity.RowKey);
            Assert.Equal("test-hint", olderAccountHintEntity.Hint);
            Assert.Equal(yesterday, olderAccountHintEntity.StartDate);

            var newerAccountHintEntity = Assert.Single(entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, userId),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.GreaterThan, $"id-{accountId}-hintDate-{yesterday:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}"),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.LessThan, "id-{accountId}-hintDate-x")
                    )
                )))
            );
            Assert.Equal("AccountHintEntity", newerAccountHintEntity.EntityType);
            Assert.Equal(userId, newerAccountHintEntity.PartitionKey);
            Assert.Equal($"id-{accountId}-hintDate-{newerAccountHintEntity.StartDate:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", newerAccountHintEntity.RowKey);
            Assert.Equal("test-hint-updated", newerAccountHintEntity.Hint);
            Assert.True(DateTime.UtcNow.AddMinutes(-1) <= newerAccountHintEntity.StartDate && newerAccountHintEntity.StartDate <= DateTime.UtcNow.AddMinutes(1));
        }

        [Fact]
        public async Task Put_WhenAccountIsDeleted_ReturnsNotFound()
        {
            var userId = Guid.NewGuid().ToString("N");
            var accountId = Guid.NewGuid().ToString("N");
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();
            var yesterday = DateTime.UtcNow;
            entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = $"name-test-name",
                    IndexedEntityId = accountId
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{accountId}",
                    Id = accountId,
                    Name = "test-name",
                    Hint = "test-hint",
                    IsPinned = false,
                    IsDeleted = true
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{accountId}-hintDate-{yesterday:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    AccountId = accountId,
                    Hint = "test-hint",
                    StartDate = yesterday
                })
            });

            var response = await client.PutAsJsonAsync($"/accounts/{accountId}", new { name = "test-name-updated", hint = "test-hint-updated", isPinned = true });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}