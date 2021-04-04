using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using Xunit;

namespace HintKeep.Tests.Integration.DeletedAccounts
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

            var response = await client.DeleteAsync("/api/deleted-accounts/%23account-id");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenAuthenticatedAndAccountDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithAuthentication("#user-id")
                .CreateClient();

            var response = await client.DeleteAsync("/api/deleted-accounts/%23account-id");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenAuthenticatedAndAccountExistsAndIsDeleted_ReturnsNoContent()
        {
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                IsDeleted = true,
                Hints = Array.Empty<AccountHint>()
            };
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithAuthentication("#user-id")
                .CreateClient();
            entityTables.AddAccounts(account);

            var response = await client.DeleteAsync($"/api/deleted-accounts/%23account-id");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            entityTables.AssertAccounts(Enumerable.Empty<Account>());
        }

        [Fact]
        public async Task Delete_WhenAuthenticatedAndAccountExistsAndIsNotDeleted_ReturnsNotFound()
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

            var response = await client.DeleteAsync($"/api/deleted-accounts/%23account-id");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            entityTables.AssertAccounts(account);
        }
    }
}