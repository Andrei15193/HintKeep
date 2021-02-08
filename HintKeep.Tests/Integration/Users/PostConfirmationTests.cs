using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class PostConfirmationTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostConfirmationTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithEmptyObject_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PostAsJsonAsync("/users/confirmations", new object());

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidEmail""],""confirmationToken"":[""validation.errors.invalidConfirmationToken""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithInvalidEmailAndConfirmationToken_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "invalid-email", confirmationToken = string.Empty });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidEmail""],""confirmationToken"":[""validation.errors.invalidConfirmationToken""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithNonExistantUser_ReturnsPreconditionFailed()
        {
            var client = _webApplicationFactory.WithInMemoryDatabase().CreateClient();

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "#eMail@DOMAIN.TLD", confirmationToken = "#token" });

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithValidEmailButExpiredConfirmationToken_ReturnsPreconditionFailed()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();

            entityTables.Logins.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new EmailLoginEntity
                {
                    EntityType = "EmailLoginEntity",
                    PartitionKey = "#email@domain.tld".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin".ToEncodedKeyProperty(),
                    State = "PendingConfirmation"
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    EntityType = "EmailLoginTokenEntity",
                    PartitionKey = "#email@domain.tld".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin-confirmationToken".ToEncodedKeyProperty(),
                    Token = "#token",
                    Created = DateTime.UtcNow.AddDays(-11)
                })
            });

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "#eMail@DOMAIN.TLD", confirmationToken = "#token" });

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithConfirmedUser_ReturnsPreconditionFailed()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();

            entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "#email@domain.tld".ToEncodedKeyProperty(),
                RowKey = "EmailLogin".ToEncodedKeyProperty(),
                State = "Confirmed"
            }));

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "#eMail@DOMAIN.TLD", confirmationToken = "#token" });

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithValidEmailAndConfirmationToken_ReturnsCreated()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .CreateClient();

            entityTables.Logins.ExecuteBatch(new TableBatchOperation{
                TableOperation.Insert(new EmailLoginEntity
                {
                    EntityType = "EmailLoginEntity",
                    PartitionKey = "#email@domain.tld".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin".ToEncodedKeyProperty(),
                    State = "PendingConfirmation"
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    EntityType = "EmailLoginTokenEntity",
                    PartitionKey = "#email@domain.tld".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin-confirmationToken".ToEncodedKeyProperty(),
                    Token = "#token",
                    Created = DateTime.UtcNow
                })
            });

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "#eMail@DOMAIN.TLD", confirmationToken = "#token" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/users", UriKind.Relative), response.Headers.Location);

            var loginEntity = (EmailLoginEntity)entityTables.Logins.Execute(TableOperation.Retrieve<EmailLoginEntity>("#email@domain.tld".ToEncodedKeyProperty(), "EmailLogin".ToEncodedKeyProperty())).Result;
            Assert.Equal("Confirmed", loginEntity.State);

            var tokenEntityQuery = new TableQuery<EmailLoginTokenEntity>()
                .Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "EmailLoginTokenEntity"))
                .Take(1);
            Assert.Empty(entityTables.Logins.ExecuteQuery<EmailLoginTokenEntity>(tokenEntityQuery));
        }
    }
}