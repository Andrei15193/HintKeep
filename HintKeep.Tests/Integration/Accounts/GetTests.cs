using System;
using System.Collections.Generic;
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
    public class GetTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public GetTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Get_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/accounts");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAuthenticated_ReturnsOkOnlyWithAuthenticatedUserAccounts()
        {
            var authenticatedUserId = Guid.NewGuid().ToString("N");
            var otherUserId = Guid.NewGuid().ToString("N");
            while (authenticatedUserId == otherUserId)
                otherUserId = Guid.NewGuid().ToString("N");
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(authenticatedUserId)
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();
            var accountsToInsert = Enumerable
                .Range(1, 5)
                .Select(accountId => (userId: authenticatedUserId, accountId: accountId.ToString()))
                .Concat(Enumerable
                    .Range(6, 5)
                    .Select(accountId => (userId: otherUserId, accountId: accountId.ToString())))
                .Select((pair, index) => (pair.userId, pair.accountId, accountNumber: index + 1));
            foreach (var (userId, accountId, accountNumber) in accountsToInsert)
            {
                var now = DateTime.UtcNow;
                entityTables.Accounts.ExecuteBatch(new TableBatchOperation
                {
                    TableOperation.Insert(new IndexEntity
                    {
                        EntityType = "IndexEntity",
                        PartitionKey = userId,
                        RowKey = $"name-test-account-{accountNumber}",
                        IndexedEntityId = accountId
                    }),
                    TableOperation.Insert(new AccountEntity
                    {
                        EntityType = "AccountEntity",
                        PartitionKey = userId,
                        RowKey = $"id-{accountId}",
                        Id = accountId,
                        Name = $"test-account-{accountNumber}",
                        Hint = $"test-hint-{accountNumber}",
                        IsPinned = true,
                        IsDeleted = false
                    }),
                    TableOperation.Insert(new AccountHintEntity
                    {
                        EntityType = "AccountHintEntity",
                        PartitionKey = userId,
                        RowKey = $"id-{accountId}-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                        AccountId = accountId,
                        StartDate = now,
                        Hint = $"test-hint-{accountNumber}",
                    })
                });
            }

            var response = await client.GetAsync("/accounts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accountsResult = await response.Content.ReadFromJsonAsync<IEnumerable<AccountGetResult>>();
            Assert.Equal(
                Enumerable.Range(1, 5).Select((accountId, index) => new
                {
                    Id = accountId.ToString(),
                    Name = $"test-account-{(index + 1)}",
                    Hint = $"test-hint-{(index + 1)}",
                    IsPinned = true
                }),
                accountsResult
                    .Select(accountResult => new
                    {
                        accountResult.Id,
                        accountResult.Name,
                        accountResult.Hint,
                        accountResult.IsPinned
                    })
                    .ToArray()
            );
        }

        [Fact]
        public async Task Get_WhenAuthenticated_ReturnsOkWithSortedAccountsByIsPinnedAndThenByName()
        {
            var userId = Guid.NewGuid().ToString("N");
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();
            entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = "name-B",
                    IndexedEntityId = "1"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = "id-1",
                    Name = "B",
                    Hint = "test-hint",
                    IsPinned = false,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = "id-1-hintDate-1",
                    AccountId = "1",
                    StartDate = DateTime.UtcNow,
                    Hint = "test-hint"
                }),
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = "name-A",
                    IndexedEntityId = "2"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = "id-2",
                    Name = "A",
                    Hint = "test-hint",
                    IsPinned = false,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = "id-2-hintDate-2",
                    AccountId = "2",
                    StartDate = DateTime.UtcNow,
                    Hint = "test-hint"
                }),
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = "name-BB",
                    IndexedEntityId = "3"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = "id-3",
                    Name = "BB",
                    Hint = "test-hint",
                    IsPinned = true,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = "id-3-hintDate-3",
                    AccountId = "3",
                    StartDate = DateTime.UtcNow,
                    Hint = "test-hint"
                }),
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = "name-AA",
                    IndexedEntityId = "4"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = "id-4",
                    Name = "AA",
                    Hint = "test-hint",
                    IsPinned = true,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = "id-4-hintDate-4",
                    AccountId = "4",
                    StartDate = DateTime.UtcNow,
                    Hint = "test-hint"
                })
            });

            var response = await client.GetAsync("/accounts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accountsResult = await response.Content.ReadFromJsonAsync<IEnumerable<AccountGetResult>>();
            Assert.Equal(
                new[]
                {
                    new
                    {
                        Name = "AA",
                        IsPinned = true
                    },
                    new
                    {
                        Name = "BB",
                        IsPinned = true
                    },
                    new
                    {
                        Name = "A",
                        IsPinned = false
                    },
                    new
                    {
                        Name = "B",
                        IsPinned = false
                    }
                },
                accountsResult
                    .Select(accountResult => new
                    {
                        accountResult.Name,
                        accountResult.IsPinned
                    })
                    .ToArray()
            );
        }

        [Fact]
        public async Task Get_WhenUserHasDeletedAccounts_ReturnsOnlyNonDeletedOnes()
        {
            var userId = Guid.NewGuid().ToString("N");
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();
            entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = "name-A",
                    IndexedEntityId = "1"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = "id-1",
                    Name = "A",
                    Hint = "test-hint",
                    IsPinned = false,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = "id-1-hintDate-1",
                    AccountId = "1",
                    StartDate = DateTime.UtcNow,
                    Hint = "test-hint"
                }),
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = "name-B",
                    IndexedEntityId = "2"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = "id-2",
                    Name = "B",
                    Hint = "test-hint",
                    IsPinned = false,
                    IsDeleted = true
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = "id-2-hintDate-2",
                    AccountId = "2",
                    StartDate = DateTime.UtcNow,
                    Hint = "test-hint"
                })
            });

            var response = await client.GetAsync("/accounts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accountsResult = await response.Content.ReadFromJsonAsync<IEnumerable<AccountGetResult>>();
            Assert.Equal(
                new[]
                {
                    new
                    {
                        Name = "A",
                        IsPinned = false
                    }
                },
                accountsResult
                    .Select(accountResult => new
                    {
                        accountResult.Name,
                        accountResult.IsPinned
                    })
                    .ToArray()
            );
        }

        private class AccountGetResult
        {
            public string Id { get; set; }
            public string Name { get; set; }

            public string Hint { get; set; }

            public bool IsPinned { get; set; }
        }
    }
}