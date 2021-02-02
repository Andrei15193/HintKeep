using System;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Queries;
using HintKeep.RequestsHandlers.Users.Queries;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Stubs;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Users.Queries
{
    public class UserAuthenticationQueryHandlerTests : IDisposable
    {
        private readonly IEntityTables _entityTables;
        private readonly Mock<ICryptographicHashService> _cryptographicHashService;
        private readonly Mock<IJsonWebTokenService> _jsonWebTokenService;
        private readonly IRequestHandler<UserAuthenticationQuery, UserInfo> _userAuthenticationQueryHandler;

        public UserAuthenticationQueryHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Logins.Create();
            _cryptographicHashService = new Mock<ICryptographicHashService>();
            _jsonWebTokenService = new Mock<IJsonWebTokenService>();
            _userAuthenticationQueryHandler = new UserAuthenticationQueryHandler(_entityTables, _cryptographicHashService.Object, _jsonWebTokenService.Object);
        }

        public void Dispose()
        {
            _cryptographicHashService.Verify();
            _cryptographicHashService.VerifyNoOtherCalls();

            _jsonWebTokenService.Verify();
            _jsonWebTokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ThrowsException()
        {
            var query = new UserAuthenticationQuery
            {
                Email = "test-eMail",
                Password = "test-password"
            };

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _userAuthenticationQueryHandler.Handle(query, default));
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenUserExistsAndPasswordsMatch_ReturnsUserInfo()
        {
            var userId = Guid.NewGuid().ToString("N");
            _cryptographicHashService
                .Setup(cryptographicHashService => cryptographicHashService.GetHash("test-salt" + "test-password"))
                .Returns("test-hash")
                .Verifiable();
            _jsonWebTokenService
                .Setup(jsonWebTokenService => jsonWebTokenService.GetJsonWebToken(userId))
                .Returns("jwt")
                .Verifiable();
            _entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "test-email",
                RowKey = "EmailLogin",
                PasswordSalt = "test-salt",
                PasswordHash = "test-hash",
                State = "Confirmed",
                UserId = userId
            }));

            var query = new UserAuthenticationQuery
            {
                Email = "test-eMail",
                Password = "test-password"
            };

            var userInfo = await _userAuthenticationQueryHandler.Handle(query, default);
            Assert.Equal("jwt", userInfo.JsonWebToken);
        }

        [Fact]
        public async Task Handle_WhenUserIsNotConfirmed_ThrowsException()
        {
            _cryptographicHashService
                .Setup(cryptographicHashService => cryptographicHashService.GetHash("test-salt" + "test-password"))
                .Returns("test-hash")
                .Verifiable();
            _entityTables.Logins.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new EmailLoginEntity
                {
                    EntityType = "EmailLoginEntity",
                    PartitionKey = "test-email",
                    RowKey = "EmailLogin",
                    PasswordSalt = "test-salt",
                    PasswordHash = "test-hash",
                    State = "PendingConfirmation",
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    EntityType = "EmailLoginTokenEntity",
                    PartitionKey = "test-email",
                    RowKey = "EmailLogin-confirmationToken",
                    Token = "token",
                    Created = DateTime.UtcNow
                })
            });

            var query = new UserAuthenticationQuery
            {
                Email = "test-eMail",
                Password = "test-password"
            };

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _userAuthenticationQueryHandler.Handle(query, default));
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenPasswordsDoNotMatch_ThrowsException()
        {
            _cryptographicHashService
                .Setup(cryptographicHashService => cryptographicHashService.GetHash("test-salt" + "test-password"))
                .Returns("test-hash-bad")
                .Verifiable();
            _entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "test-email",
                RowKey = "EmailLogin",
                PasswordSalt = "test-salt",
                PasswordHash = "test-hash",
                State = "Confirmed"
            }));

            var query = new UserAuthenticationQuery
            {
                Email = "test-eMail",
                Password = "test-password"
            };

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _userAuthenticationQueryHandler.Handle(query, default));
            Assert.Empty(exception.Message);
        }
    }
}