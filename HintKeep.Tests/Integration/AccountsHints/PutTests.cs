using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.AccountsHints
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

            var response = await client.PutAsJsonAsync("/api/accounts/%23account-id/hints/%23hint-id", string.Empty);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WithEmptyObject_ReturnsNoContent()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id"
                    }
                }
            });

            var response = await client.PutAsJsonAsync("/api/accounts/%23account-id/hints/%23hint-id", new object());

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("#user-id".ToEncodedKeyProperty(), "accountId-#account-id".ToEncodedKeyProperty())).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("#user-id".ToEncodedKeyProperty(), accountEntity.PartitionKey);
            Assert.Equal("accountId-#account-id".ToEncodedKeyProperty(), accountEntity.RowKey);
            Assert.Equal("#account-id", accountEntity.Id);
            Assert.Equal("#Test-Name", accountEntity.Name);
            Assert.Null(accountEntity.Hint);
            Assert.Equal("#Test-Notes", accountEntity.Notes);
            Assert.True(accountEntity.IsPinned);
            Assert.False(accountEntity.IsDeleted);

            var accountHintEntity = Assert.Single(entityTables
                .AccountHints
                .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")))
            );
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("accountId-#account-id".ToEncodedKeyProperty(), accountHintEntity.PartitionKey);
            Assert.Equal("hintId-#hint-id".ToEncodedKeyProperty(), accountHintEntity.RowKey);
            Assert.Equal("#account-id", accountHintEntity.AccountId);
            Assert.Equal("#hint-id", accountHintEntity.HintId);
            Assert.Equal("#Test-Hint", accountHintEntity.Hint);
            Assert.Null(accountHintEntity.DateAdded);
        }

        [Fact]
        public async Task Put_WithDateAdded_ReturnsNoContent()
        {
            var now = DateTime.UtcNow;
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id",
                        DateAdded = now.AddDays(-1)
                    }
                }
            });

            var response = await client.PutAsJsonAsync("/api/accounts/%23account-id/hints/%23hint-id", new { dateAdded = now });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("#user-id".ToEncodedKeyProperty(), "accountId-#account-id".ToEncodedKeyProperty())).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("#user-id".ToEncodedKeyProperty(), accountEntity.PartitionKey);
            Assert.Equal("accountId-#account-id".ToEncodedKeyProperty(), accountEntity.RowKey);
            Assert.Equal("#Test-Name", accountEntity.Name);
            Assert.Null(accountEntity.Hint);
            Assert.Equal("#Test-Notes", accountEntity.Notes);

            var accountHintEntity = Assert.Single(entityTables
                .AccountHints
                .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")))
            );
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("accountId-#account-id".ToEncodedKeyProperty(), accountHintEntity.PartitionKey);
            Assert.Equal("hintId-#hint-id".ToEncodedKeyProperty(), accountHintEntity.RowKey);
            Assert.Equal("#account-id", accountHintEntity.AccountId);
            Assert.Equal("#hint-id", accountHintEntity.HintId);
            Assert.Equal("#Test-Hint", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.DateAdded);
        }

        [Fact]
        public async Task Put_WithEarlierDateAdded_ReturnsNoContentAndUpdatesAccountWithNewLatestHint()
        {
            var now = DateTime.UtcNow;
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id-1",
                        Hint = "#Test-Hint-1",
                        DateAdded = now.AddDays(-1)
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        Hint = "#Test-Hint-2",
                        DateAdded = now.AddDays(-2)
                    }
                }
            });

            var response = await client.PutAsJsonAsync("/api/accounts/%23account-id/hints/%23hint-id-2", new { dateAdded = now });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("#user-id".ToEncodedKeyProperty(), "accountId-#account-id".ToEncodedKeyProperty())).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("#user-id".ToEncodedKeyProperty(), accountEntity.PartitionKey);
            Assert.Equal("accountId-#account-id".ToEncodedKeyProperty(), accountEntity.RowKey);
            Assert.Equal("#Test-Name", accountEntity.Name);
            Assert.Null(accountEntity.Hint);
            Assert.Equal("#Test-Notes", accountEntity.Notes);

            var accountHintEntity = entityTables
                .AccountHints
                .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")))
                .OrderByDescending(hint => hint.DateAdded)
                .First();
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("accountId-#account-id".ToEncodedKeyProperty(), accountHintEntity.PartitionKey);
            Assert.Equal("hintId-#hint-id-2".ToEncodedKeyProperty(), accountHintEntity.RowKey);
            Assert.Equal("#account-id", accountHintEntity.AccountId);
            Assert.Equal("#hint-id-2", accountHintEntity.HintId);
            Assert.Equal("#Test-Hint-2", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.DateAdded);
        }

        [Fact]
        public async Task Put_WithNoDateAdded_ReturnsNoContentAndUpdatesAccountWithNewLatestHint()
        {
            var now = DateTime.UtcNow;
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id-1",
                        Hint = "#Test-Hint-1",
                        DateAdded = now.AddDays(-1)
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        Hint = "#Test-Hint-2",
                        DateAdded = now.AddDays(-2)
                    }
                }
            });

            var response = await client.PutAsJsonAsync("/api/accounts/%23account-id/hints/%23hint-id-1", new object());

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("#user-id".ToEncodedKeyProperty(), "accountId-#account-id".ToEncodedKeyProperty())).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("#user-id".ToEncodedKeyProperty(), accountEntity.PartitionKey);
            Assert.Equal("accountId-#account-id".ToEncodedKeyProperty(), accountEntity.RowKey);
            Assert.Equal("#Test-Name", accountEntity.Name);
            Assert.Null(accountEntity.Hint);
            Assert.Equal("#Test-Notes", accountEntity.Notes);

            var accountHintEntity = Assert.Single(
                entityTables
                    .AccountHints
                    .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity"))),
                hint => hint.DateAdded is null
            );
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("accountId-#account-id".ToEncodedKeyProperty(), accountHintEntity.PartitionKey);
            Assert.Equal("hintId-#hint-id-1".ToEncodedKeyProperty(), accountHintEntity.RowKey);
            Assert.Equal("#account-id", accountHintEntity.AccountId);
            Assert.Equal("#hint-id-1", accountHintEntity.HintId);
            Assert.Equal("#Test-Hint-1", accountHintEntity.Hint);
            Assert.Null(accountHintEntity.DateAdded);
        }

        [Fact]
        public async Task Put_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.PutAsJsonAsync("/api/accounts/%23account-id/hints/%23hint-id-1", new object());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WhenHintDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id"
                    }
                }
            });

            var response = await client.PutAsJsonAsync("/api/accounts/%23account-id/hints/%23hint-id-invalid", new object());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_WhenAccountIsDeleted_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id"
                    }
                },
                IsDeleted = true
            });

            var response = await client.PutAsJsonAsync("/api/accounts/%23account-id/hints/%23hint-id", new object());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}