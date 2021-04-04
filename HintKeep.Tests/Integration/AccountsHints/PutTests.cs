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
            var accountHintEntity = Assert.Single(entityTables
                .Accounts
                .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")))
            );
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("#user-id", accountHintEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("id-#account-id-hintId-#hint-id", accountHintEntity.RowKey.FromEncodedKeyProperty());
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
            var accountHintEntity = Assert.Single(entityTables
                .Accounts
                .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")))
            );
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("#user-id", accountHintEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("id-#account-id-hintId-#hint-id", accountHintEntity.RowKey.FromEncodedKeyProperty());
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
            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("#user-id".ToEncodedKeyProperty(), "id-#account-id".ToEncodedKeyProperty())).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("#user-id", accountEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("id-#account-id", accountEntity.RowKey.FromEncodedKeyProperty());
            Assert.Equal("#Test-Name", accountEntity.Name);
            Assert.Equal("#Test-Hint-2", accountEntity.Hint);
            Assert.Equal("#Test-Notes", accountEntity.Notes);

            var accountHintEntity = entityTables
                .Accounts
                .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")))
                .OrderByDescending(hint => hint.DateAdded)
                .First();
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("#user-id", accountHintEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("id-#account-id-hintId-#hint-id-2", accountHintEntity.RowKey.FromEncodedKeyProperty());
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
            var accountEntity = (AccountEntity)entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("#user-id".ToEncodedKeyProperty(), "id-#account-id".ToEncodedKeyProperty())).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("#user-id", accountEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("id-#account-id", accountEntity.RowKey.FromEncodedKeyProperty());
            Assert.Equal("#Test-Name", accountEntity.Name);
            Assert.Equal("#Test-Hint-2", accountEntity.Hint);
            Assert.Equal("#Test-Notes", accountEntity.Notes);

            var accountHintEntity = Assert.Single(
                entityTables
                    .Accounts
                    .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity"))),
                hint => hint.DateAdded is null
            );
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("#user-id", accountHintEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("id-#account-id-hintId-#hint-id-1", accountHintEntity.RowKey.FromEncodedKeyProperty());
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