using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
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

            var response = await client.GetAsync("/api/accounts");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAuthenticated_ReturnsOkOnlyWithAuthenticatedUserAccounts()
        {
            var userAccounts = Enumerable
                .Range(1, 5)
                .Select(accountNumber => new Account
                {
                    UserId = "user-id",
                    Name = $"#Test-Name-{accountNumber}",
                    Hints = new[]
                    {
                        new AccountHint { Hint = $"#Test-Hint-{accountNumber}" }
                    },
                    IsPinned = true
                })
                .ToArray()
                .AsEnumerable();
            var otherUserAccounts = Enumerable
                .Range(6, 5)
                .Select(accountNumber => new Account
                {
                    UserId = "other-user-id",
                    Name = $"#Test-Name-{accountNumber}",
                    Hints = new[]
                    {
                        new AccountHint{ Hint = $"#Test-Hint-{accountNumber}" }
                    },
                    IsPinned = true
                })
                .ToArray()
                .AsEnumerable();
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("user-id")
                .CreateClient();
            entityTables
                .AddAccounts(userAccounts)
                .AddAccounts(otherUserAccounts);

            var response = await client.GetAsync("/api/accounts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accountsResult = await response.Content.ReadFromJsonAsync<IEnumerable<AccountGetResult>>();
            Assert.Equal(
                userAccounts
                    .Select(account => new
                    {
                        account.Id,
                        account.Name,
                        account.Hints.Single().Hint,
                        account.IsPinned
                    })
                    .ToArray(),
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
            var accounts = new[]
            {
                new Account
                {
                    UserId = userId,
                    Name = "B",
                    IsPinned = false
                },
                new Account
                {
                    UserId = userId,
                    Name = "A",
                    IsPinned = false
                },
                new Account
                {
                    UserId = userId,
                    Name = "BB",
                    IsPinned = true
                },
                new Account
                {
                    UserId = userId,
                    Name = "AA",
                    IsPinned = true
                }
            };
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication(userId)
                .CreateClient();
            entityTables.AddAccounts(accounts);

            var response = await client.GetAsync("/api/accounts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accountsResult = await response.Content.ReadFromJsonAsync<IEnumerable<AccountGetResult>>();
            Assert.Equal(
                accounts
                    .OrderByDescending(account => account.IsPinned)
                    .ThenBy(account => account.Name)
                    .Select(account => new
                    {
                        account.Id,
                        account.Name,
                        account.Hints.Single().Hint,
                        account.IsPinned
                    })
                    .ToArray(),
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
        public async Task Get_WhenUserHasDeletedAccounts_ReturnsOnlyNonDeletedOnes()
        {
            var userId = Guid.NewGuid().ToString("N");
            var accounts = new[]
            {
                new Account
                {
                    UserId = userId,
                    Name = "A"
                },
                new Account
                {
                    UserId = userId,
                    Name = "B",
                    IsDeleted = true
                }
            };
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication(userId)
                .CreateClient();
            entityTables.AddAccounts(accounts);

            var response = await client.GetAsync("/api/accounts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accountsResult = await response.Content.ReadFromJsonAsync<IEnumerable<AccountGetResult>>();
            Assert.Equal(
                accounts
                    .Take(1)
                    .Select(account => new
                    {
                        account.Id,
                        account.Name,
                        account.Hints.Single().Hint,
                        account.IsPinned
                    })
                    .ToArray(),
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

        private class AccountGetResult
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Hint { get; set; }

            public bool IsPinned { get; set; }
        }
    }
}