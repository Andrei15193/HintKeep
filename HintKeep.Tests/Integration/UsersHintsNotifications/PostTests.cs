using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Xunit;

namespace HintKeep.Tests.Integration.UsersHintsNotifications
{
    public class PostTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public PostTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Post_WithInvalidEmail_ReturnsUnprocessableEntity()
        {
            var client = _webApplicationFactory
                .CreateClient();

            var response = await client.PostAsJsonAsync("/api/users/hints/notifications", new { email = "invalid-email-address" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal(@"{""email"":[""validation.errors.invalidRequiredEmailAddress""]}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenActiveUserExists_ReturnsCreated()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithSecurityService(out var securityService)
                .WithEmailService(out var emailService)
                .CreateClient();
            entityTables.Users.ExecuteBatch(
                new TableBatchOperation
                {
                    TableOperation.Insert(new UserEntity
                    {
                        PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                        RowKey = "details".ToEncodedKeyProperty(),
                        EntityType = "UserEntity",
                        Hint = "#hint",
                        IsActive = true,
                    })
                }
            );
            securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            var response = await client.PostAsJsonAsync("/api/users/hints/notifications", new { email = "#TEST@domain.com" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(new Uri("/api/users/sessions", UriKind.Relative), response.Headers.Location);

            emailService.Verify(emailService => emailService.SendAsync("#TEST@domain.com", "HintKeep - Account Hint", It.IsRegex("#hint")), Times.Once);
            emailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Post_WhenUserDoesNotExist_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase()
                .WithSecurityService(out var securityService)
                .CreateClient();
            securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            var response = await client.PostAsJsonAsync("/api/users/hints/notifications", new { email = "#TEST@domain.com" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_WhenInactiveUserExists_ReturnsNotFound()
        {
            var client = _webApplicationFactory
                .WithInMemoryDatabase(out var entityTables)
                .WithSecurityService(out var securityService)
                .CreateClient();
            entityTables.Users.ExecuteBatch(
                new TableBatchOperation
                {
                    TableOperation.Insert(new UserEntity
                    {
                        PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                        RowKey = "details".ToEncodedKeyProperty(),
                        EntityType = "UserEntity",
                        IsActive = false,
                    })
                }
            );
            securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            var response = await client.PostAsJsonAsync("/api/users/hints/notifications", new { email = "#TEST@domain.com" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }
}