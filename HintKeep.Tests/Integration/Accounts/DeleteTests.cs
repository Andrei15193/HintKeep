using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Accounts
{
    public class DeleteTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public DeleteTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
        {
            var accountId = Guid.NewGuid().ToString("N");
            var client = _webApplicationFactory.CreateClient();

            var response = await client.DeleteAsync($"/accounts/{accountId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenAuthenticatedAndAccountDoesNotExist_ReturnsNotFound()
        {
            var userId = Guid.NewGuid().ToString("N");
            var accountId = Guid.NewGuid().ToString("N");
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(actualEntityTable => entityTables = actualEntityTable)
                .CreateClient();

            var response = await client.DeleteAsync($"/accounts/{accountId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenAuthenticatedAndAccountExists_ReturnsNoContent()
        {
            var userId = Guid.NewGuid().ToString("N");
            var accountId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(actualEntityTable => entityTables = actualEntityTable)
                .CreateClient();
            entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = "name-test-account",
                    IndexedEntityId = accountId
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{accountId}",
                    Id = accountId,
                    Name= "test-account",
                    Hint = "test-hint",
                    IsPinned = true,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{accountId}-hintDate-1",
                    AccountId = accountId,
                    Hint = "test-hint",
                    StartDate = now
                })
            });

            var response = await client.DeleteAsync($"/accounts/{accountId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            Assert.Equal(3, entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexedEntity = (IndexEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(userId, "name-test-account")).Result;
            Assert.Equal("IndexEntity", indexedEntity.EntityType);
            Assert.Equal(userId, indexedEntity.PartitionKey);
            Assert.Equal("name-test-account", indexedEntity.RowKey);
            Assert.NotEmpty(indexedEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(userId, $"id-{indexedEntity.IndexedEntityId}")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal(userId, accountEntity.PartitionKey);
            Assert.Equal($"id-{indexedEntity.IndexedEntityId}", accountEntity.RowKey);
            Assert.Equal(indexedEntity.IndexedEntityId, accountEntity.Id);
            Assert.Equal("test-account", accountEntity.Name);
            Assert.Equal("test-hint", accountEntity.Hint);
            Assert.True(accountEntity.IsPinned);
            Assert.True(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>(userId, $"id-{accountId}-hintDate-1")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal(userId, accountHintEntity.PartitionKey);
            Assert.Equal($"id-{indexedEntity.IndexedEntityId}-hintDate-1", accountHintEntity.RowKey);
            Assert.Equal(now, accountHintEntity.StartDate);
            Assert.Equal(accountEntity.Id, accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
        }

        [Fact]
        public async Task Delete_WhenAuthenticatedAndAccountIsDeleted_ReturnsNotFound()
        {
            var userId = Guid.NewGuid().ToString("N");
            var accountId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(actualEntityTable => entityTables = actualEntityTable)
                .CreateClient();
            entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = "name-test-account",
                    IndexedEntityId = accountId
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{accountId}",
                    Id = accountId,
                    Name= "test-account",
                    Hint = "test-hint",
                    IsPinned = true,
                    IsDeleted = true
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{accountId}-hintDate-1",
                    AccountId = accountId,
                    Hint = "test-hint",
                    StartDate = now
                })
            });

            var response = await client.DeleteAsync($"/accounts/{accountId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            Assert.Equal(3, entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexedEntity = (IndexEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(userId, "name-test-account")).Result;
            Assert.Equal("IndexEntity", indexedEntity.EntityType);
            Assert.Equal(userId, indexedEntity.PartitionKey);
            Assert.Equal("name-test-account", indexedEntity.RowKey);
            Assert.NotEmpty(indexedEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(userId, $"id-{indexedEntity.IndexedEntityId}")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal(userId, accountEntity.PartitionKey);
            Assert.Equal($"id-{indexedEntity.IndexedEntityId}", accountEntity.RowKey);
            Assert.Equal(indexedEntity.IndexedEntityId, accountEntity.Id);
            Assert.Equal("test-account", accountEntity.Name);
            Assert.Equal("test-hint", accountEntity.Hint);
            Assert.True(accountEntity.IsPinned);
            Assert.True(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>(userId, $"id-{accountId}-hintDate-1")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal(userId, accountHintEntity.PartitionKey);
            Assert.Equal($"id-{indexedEntity.IndexedEntityId}-hintDate-1", accountHintEntity.RowKey);
            Assert.Equal(now, accountHintEntity.StartDate);
            Assert.Equal(accountEntity.Id, accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
        }
    }
}