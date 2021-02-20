using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using Xunit;

namespace HintKeep.Tests.Integration.AccountsHints
{
    public class DeleteTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public DeleteTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.DeleteAsync("/accounts/%23account-id/hints/%23hint-id");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenReferredHintIsOlder_ReturnsNoContent()
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
                        Id = "#hint-id-1",
                        Hint = "#Test-Hint-1",
                        DateAdded = now
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        Hint = "#Test-Hint-2",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            entityTables.AddAccounts(account);

            var response = await client.DeleteAsync("/accounts/%23account-id/hints/%23hint-id-2");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    account.Hints.First()
                }
            });
        }

        [Fact]
        public async Task Delete_WhenReferredHintIsNewer_ReturnsNoContent()
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
                        Id = "#hint-id-1",
                        Hint = "#Test-Hint-1",
                        DateAdded = now
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        Hint = "#Test-Hint-2",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            entityTables.AddAccounts(account);

            var response = await client.DeleteAsync("/accounts/%23account-id/hints/%23hint-id-1");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    account.Hints.Last()
                }
            });
        }

        [Fact]
        public async Task Delete_WhenThereIsOnlyOneHint_ReturnsNoContent()
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
                        Id = "#hint-id",
                        Hint = "#Test-Hint",
                        DateAdded = DateTime.UtcNow
                    }
                }
            };
            entityTables.AddAccounts(account);

            var response = await client.DeleteAsync("/accounts/%23account-id/hints/%23hint-id");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(new Account(account)
            {
                Hints = Array.Empty<AccountHint>()
            });
        }

        [Fact]
        public async Task Delete_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            var now = DateTime.UtcNow;
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.DeleteAsync("/accounts/%23account-id/hints/%23hint-id");

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
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id"
                    }
                },
                IsDeleted = true
            };
            entityTables.AddAccounts(account);

            var response = await client.DeleteAsync("/accounts/%23account-id/hints/%23hint-id");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(account);
        }

        [Fact]
        public async Task Post_WhenAccountHintDoesNotExist_ReturnsNotFound()
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
                        Id = "#hint-id"
                    }
                }
            };
            entityTables.AddAccounts(account);

            var response = await client.DeleteAsync("/accounts/%23account-id/hints/%23hint-id-not-existing");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(account);
        }
    }
}