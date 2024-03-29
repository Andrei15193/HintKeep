using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using Xunit;

namespace HintKeep.Tests.Integration.DeletedAccounts
{
    public class GetIdTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public GetIdTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Get_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/api/deleted-accounts/%23account-id");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.GetAsync("/api/deleted-accounts/%23account-id");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAccountExistAndIsDeleted_ReturnsOk()
        {
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                IsDeleted = true
            };
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(account);

            var response = await client.GetAsync($"/api/deleted-accounts/%23account-id");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accountResult = await response.Content.ReadFromJsonAsync<AccountGetResult>();
            Assert.Equal(
                new
                {
                    Id = account.Id,
                    Name = account.Name,
                    Hint = account.LatestHint,
                    Notes = account.Notes,
                    IsPinned = account.IsPinned
                },
                new
                {
                    accountResult.Id,
                    accountResult.Name,
                    accountResult.Hint,
                    accountResult.Notes,
                    accountResult.IsPinned
                }
            );
        }

        [Fact]
        public async Task Get_WhenAccountExistAndIsNotDeleted_ReturnsNotFound()
        {
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                IsDeleted = false
            };
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(account);

            var response = await client.GetAsync($"/api/deleted-accounts/%23account-id");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_WhenAccountExistForDifferentUser_ReturnsNotFound()
        {
            var account = new Account
            {
                UserId = "#other-user-id",
                Id = "#account-id",
                IsDeleted = true
            };
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(account);

            var response = await client.GetAsync($"/api/deleted-accounts/%23account-id");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        private class AccountGetResult
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Hint { get; set; }

            public string Notes { get; set; }

            public bool IsPinned { get; set; }
        }
    }
}