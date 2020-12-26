using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Integration.Users
{
    public class PostConfirmationTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly WebApplicationFactory<Startup> _webApplicationFactory;

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

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "eMail@DOMAIN.TLD", confirmationToken = "token" });

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithValidEmailButExpiredConfirmationToken_ReturnsPreconditionFailed()
        {
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();

            entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "email@domain.tld",
                    RowKey = "user",
                    State = (int)UserState.Confirmed
                }),
                TableOperation.Insert(new TokenEntity
                {
                    PartitionKey = "email@domain.tld",
                    RowKey = "confirmation_tokens-token",
                    Token = "token",
                    Intent = (int)TokenIntent.ConfirmUserRegistration,
                    Created = DateTime.UtcNow.AddDays(-10)
                })
            });

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "eMail@DOMAIN.TLD", confirmationToken = "token" });

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithValidEmailButConfirmationTokenOfDifferentIntent_ReturnsPreconditionFailed()
        {
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();

            entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "email@domain.tld",
                    RowKey = "user",
                    State = (int)UserState.Confirmed
                }),
                TableOperation.Insert(new TokenEntity
                {
                    PartitionKey = "email@domain.tld",
                    RowKey = "confirmation_tokens-token",
                    Token = "token",
                    Intent = -1,
                    Created = DateTime.UtcNow.AddMinutes(-1)
                })
            });

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "eMail@DOMAIN.TLD", confirmationToken = "token" });

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithConfirmedUser_ReturnsPreconditionFailed()
        {
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();

            entityTables.Users.Execute(TableOperation.Insert(new UserEntity
            {
                PartitionKey = "email@domain.tld",
                RowKey = "user",
                State = (int)UserState.Confirmed
            }));

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "eMail@DOMAIN.TLD", confirmationToken = "token" });

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WithValidEmailAndConfirmationToken_ReturnsCreated()
        {
            var entityTables = default(IEntityTables);
            var client = _webApplicationFactory
                .WithInMemoryDatabase(actualEntityTables => entityTables = actualEntityTables)
                .CreateClient();

            entityTables.Users.ExecuteBatch(new TableBatchOperation{
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "email@domain.tld",
                    RowKey = "user",
                    State = (int)UserState.PendingConfirmation
                }),
                TableOperation.Insert(new TokenEntity
                {
                    PartitionKey = "email@domain.tld",
                    RowKey = "confirmation_tokens-token",
                    Token = "token",
                    Intent = (int)TokenIntent.ConfirmUserRegistration,
                    Created = DateTime.UtcNow.AddMinutes(-1)
                })
            });

            var response = await client.PostAsJsonAsync("/users/confirmations", new { email = "eMail@DOMAIN.TLD", confirmationToken = "token" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/users", UriKind.Relative), response.Headers.Location);

            var userEntity = (UserEntity)entityTables.Users.Execute(TableOperation.Retrieve<UserEntity>("email@domain.tld", "user")).Result;
            Assert.Equal(UserState.Confirmed, (UserState)userEntity.State);

            var tokenEntityQuery = new TableQuery<TokenEntity>()
                .Where(TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.NotEqual, "user"))
                .Take(1);
            Assert.Empty(entityTables.Users.ExecuteQuery<TokenEntity>(tokenEntityQuery));
        }
    }
}