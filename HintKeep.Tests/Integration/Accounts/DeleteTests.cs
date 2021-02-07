using System.Net;
using System.Threading.Tasks;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using Xunit;

namespace HintKeep.Tests.Integration.Accounts
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

            var response = await client.DeleteAsync($"/accounts/account-id");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenAuthenticatedAndAccountDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithAuthentication("user-id")
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();

            var response = await client.DeleteAsync($"/accounts/account-id");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_WhenAuthenticatedAndAccountExists_ReturnsNoContent()
        {
            var account = new Account();
            var client = _webApplicationFactory
                .WithAuthentication(account.UserId)
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();

            entityTables.AddAccounts(account);

            var response = await client.DeleteAsync($"/accounts/{account.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            entityTables.AssertAccounts(new Account(account) { IsDeleted = true });
        }

        [Fact]
        public async Task Delete_WhenAuthenticatedAndAccountIsDeleted_ReturnsNotFound()
        {
            var account = new Account { IsDeleted = true };
            var client = _webApplicationFactory
                .WithAuthentication(account.UserId)
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();
            entityTables.AddAccounts(account);

            var response = await client.DeleteAsync($"/accounts/{account.Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());

            entityTables.AssertAccounts(account);
        }
    }
}