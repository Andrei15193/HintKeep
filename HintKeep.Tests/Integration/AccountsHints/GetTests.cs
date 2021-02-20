using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using Xunit;

namespace HintKeep.Tests.Integration.AccountsHints
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

            var response = await client.GetAsync("/accounts/%23account-id/hints");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAccountHasHints_ReturnsThemSortedByDateAddedThenByName()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();

            var now = DateTime.UtcNow;
            var accounts = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id-5",
                        DateAdded = null,
                        Hint = "#Test-Hint-5"
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        DateAdded = now,
                        Hint = "#Test-Hint-2"
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-1",
                        DateAdded = now,
                        Hint = "#Test-Hint-1"
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-3",
                        DateAdded = now,
                        Hint = "#Test-Hint-3"
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-4",
                        DateAdded = now.AddDays(-1),
                        Hint = "#Test-Hint-4"
                    }
                }
            };
            entityTables.AddAccounts(accounts);

            var response = await client.GetAsync("/accounts/%23account-id/hints");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accountHintsResult = await response.Content.ReadFromJsonAsync<IEnumerable<AccountHintGetResult>>();
            Assert.Equal(
                new[]
                {
                    new
                    {
                        Id = "#hint-id-1",
                        Hint = "#Test-Hint-1",
                        DateAdded = (DateTime?)now
                    },
                    new
                    {
                        Id = "#hint-id-2",
                        Hint = "#Test-Hint-2",
                        DateAdded = (DateTime?)now
                    },
                    new
                    {
                        Id = "#hint-id-3",
                        Hint = "#Test-Hint-3",
                        DateAdded = (DateTime?)now
                    },
                    new
                    {
                        Id = "#hint-id-4",
                        Hint = "#Test-Hint-4",
                        DateAdded = (DateTime?)now.AddDays(-1)
                    },
                    new
                    {
                        Id = "#hint-id-5",
                        Hint = "#Test-Hint-5",
                        DateAdded = default(DateTime?)
                    }
                },
                accountHintsResult
                    .Select(accountHint => new
                    {
                        accountHint.Id,
                        accountHint.Hint,
                        accountHint.DateAdded
                    })
                    .ToArray()
            );
        }

        [Fact]
        public async Task Get_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            var now = DateTime.UtcNow;
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.GetAsync("/accounts/%23account-id/hints");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAccountIsDeleted_ReturnsNotFound()
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

            var response = await client.GetAsync("/accounts/%23account-id/hints");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            entityTables.AssertAccounts(account);
        }

        private class AccountHintGetResult
        {
            public string Id { get; set; }

            public string Hint { get; set; }

            public DateTime? DateAdded { get; set; }
        }
    }
}