
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Accounts
{
    public class GetIdTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly string _accountId = Guid.NewGuid().ToString("N");
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public GetIdTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Get_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync($"/accounts/{_accountId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            var authenticatedUserId = Guid.NewGuid().ToString("N");
            var client = _webApplicationFactory
                .WithAuthentication(authenticatedUserId)
                .WithInMemoryDatabase()
                .CreateClient();

            var response = await client.GetAsync($"/accounts/{_accountId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAccountExist_ReturnsNotFound()
        {
            var userId = Guid.NewGuid().ToString("N");
            var client = _webApplicationFactory
                .WithAuthentication(userId)
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();
            entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = userId,
                    RowKey = "name-test-name",
                    IndexedEntityId = _accountId
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{_accountId}",
                    Id = _accountId,
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = true,
                    IsDeleted = true
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = userId,
                    RowKey = $"id-{_accountId}-hintDate-1",
                    AccountId = _accountId,
                    StartDate = DateTime.UtcNow,
                    Hint = "test-hint"
                })
            });

            var response = await client.GetAsync($"/accounts/{_accountId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accountResult = await response.Content.ReadFromJsonAsync<AccountGetResult>();
            Assert.Equal(
                new
                {
                    Id = _accountId,
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = true,
                    IsDeleted = true
                },
                new
                {
                    accountResult.Id,
                    accountResult.Name,
                    accountResult.Hint,
                    accountResult.Notes,
                    accountResult.IsPinned,
                    accountResult.IsDeleted
                }
            );
        }

        private class AccountGetResult
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Hint { get; set; }

            public string Notes { get; set; }

            public bool IsPinned { get; set; }

            public bool IsDeleted { get; set; }
        }
    }
}