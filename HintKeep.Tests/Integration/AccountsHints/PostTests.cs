using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.AccountsHints
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

            var response = await client.PostAsJsonAsync("/accounts/%23account-id/hints", string.Empty);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithEmptyObject_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id"
            };
            entityTables.AddAccounts(account);

            var response = await client.PostAsJsonAsync("/accounts/%23account-id/hints", new object());
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""]}", await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(account);
        }

        [Fact]
        public async Task Post_WithInvalidHint_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id"
            };
            entityTables.AddAccounts(account);

            var response = await client.PostAsJsonAsync("/accounts/%23account-id/hints", new { hint = " " });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""hint"":[""validation.errors.invalidRequiredMediumText""]}", await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(account);
        }

        [Fact]
        public async Task Post_WithValidHint_ReturnsCreated()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = Array.Empty<AccountHint>()
            };
            entityTables.AddAccounts(account);

            var response = await client.PostAsJsonAsync("/accounts/%23account-id/hints", new { hint = "#Test-Hint" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            var accountHintEntity = Assert.Single(entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(
                TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")
            )));

            Assert.Equal(new Uri($"/accounts/#account-id/hints/{accountHintEntity.HintId}", UriKind.Relative), response.Headers.Location);
            entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = accountHintEntity.HintId,
                        Hint = "#Test-Hint",
                        DateAdded = null
                    }
                }
            });
        }

        [Fact]
        public async Task Post_WithNewerHint_ReturnsCreated()
        {
            var now = DateTime.UtcNow;
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Hint = "#Test-Hint-Older",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            entityTables.AddAccounts(account);

            var response = await client.PostAsJsonAsync("/accounts/%23account-id/hints", new { hint = "#Test-Hint-Newer", dateAdded = now });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            var accountHintEntity = Assert.Single(
                entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity"))),
                accountHint => accountHint.DateAdded == now
            );

            Assert.Equal(new Uri($"/accounts/#account-id/hints/{accountHintEntity.HintId}", UriKind.Relative), response.Headers.Location);
            entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = accountHintEntity.HintId,
                        Hint = "#Test-Hint-Newer",
                        DateAdded = now
                    },
                    account.Hints.Single()
                }
            });
        }

        [Fact]
        public async Task Post_WithOlderHint_ReturnsCreated()
        {
            var now = DateTime.UtcNow;
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Hint = "#Test-Hint-Older",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            entityTables.AddAccounts(account);

            var response = await client.PostAsJsonAsync("/accounts/%23account-id/hints", new { hint = "#Test-Hint-Newer", dateAdded = now.AddDays(-2) });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            var accountHintEntity = Assert.Single(
                entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity"))),
                accountHint => accountHint.DateAdded == now.AddDays(-2)
            );

            Assert.Equal(new Uri($"/accounts/#account-id/hints/{accountHintEntity.HintId}", UriKind.Relative), response.Headers.Location);
            entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    account.Hints.Single(),
                    new AccountHint
                    {
                        Id = accountHintEntity.HintId,
                        Hint = "#Test-Hint-Newer",
                        DateAdded = now.AddDays(-2)
                    }
                }
            });
        }

        [Fact]
        public async Task Post_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            var now = DateTime.UtcNow;
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.PostAsJsonAsync("/accounts/%23account-id/hints", new { hint = "#Test-Hint" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenAccountIsDeleted_ReturnsNotFound()
        {
            var now = DateTime.UtcNow;
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                IsDeleted = true
            };
            entityTables.AddAccounts(account);

            var response = await client.PostAsJsonAsync("/accounts/%23account-id/hints", new { hint = "#Test-Hint" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(account);
        }
    }
}