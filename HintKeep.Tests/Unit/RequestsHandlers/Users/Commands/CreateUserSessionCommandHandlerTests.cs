using System;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Commands;
using HintKeep.RequestsHandlers.Users.Commands;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Stubs;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Users.Commands
{
    public class CreateUserSessionCommandHandlerTests : IDisposable
    {
        private readonly IEntityTables _entityTables;
        private readonly Mock<ICryptographicHashService> _cryptographicHashService;
        private readonly Mock<IJsonWebTokenService> _jsonWebTokenService;
        private readonly IRequestHandler<CreateUserSessionCommand, UserSession> _createUserSessionCommandHandler;

        public CreateUserSessionCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Logins.Create();
            _entityTables.UserSessions.Create();
            _cryptographicHashService = new Mock<ICryptographicHashService>();
            _jsonWebTokenService = new Mock<IJsonWebTokenService>();
            _createUserSessionCommandHandler = new CreateUserSessionCommandHandler(_entityTables, _cryptographicHashService.Object, _jsonWebTokenService.Object);
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
            var command = new CreateUserSessionCommand
            {
                Email = "#test-eMail",
                Password = "#test-password"
            };

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _createUserSessionCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenUserExistsAndPasswordsMatch_ReturnsSessionInfo()
        {
            _cryptographicHashService
                .Setup(cryptographicHashService => cryptographicHashService.GetHash("#test-salt#test-password"))
                .Returns("#test-hash")
                .Verifiable();
            _jsonWebTokenService
                .Setup(jsonWebTokenService => jsonWebTokenService.GetJsonWebToken("#user-id", It.IsAny<string>()))
                .Returns("jwt")
                .Verifiable();
            _entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "#test-email".ToEncodedKeyProperty(),
                RowKey = "EmailLogin".ToEncodedKeyProperty(),
                PasswordSalt = "#test-salt",
                PasswordHash = "#test-hash",
                State = "Confirmed",
                UserId = "#user-id"
            }));

            var command = new CreateUserSessionCommand
            {
                Email = "#test-eMail",
                Password = "#test-password"
            };

            var userSessionInfo = await _createUserSessionCommandHandler.Handle(command, default);
            Assert.Equal("jwt", userSessionInfo.JsonWebToken);
        }

        [Fact]
        public async Task Handle_WhenUserIsNotConfirmed_ThrowsException()
        {
            _cryptographicHashService
                .Setup(cryptographicHashService => cryptographicHashService.GetHash("#test-salt" + "#test-password"))
                .Returns("#test-hash")
                .Verifiable();
            _entityTables.Logins.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new EmailLoginEntity
                {
                    EntityType = "EmailLoginEntity",
                    PartitionKey = "#test-email".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin".ToEncodedKeyProperty(),
                    PasswordSalt = "#test-salt",
                    PasswordHash = "#test-hash",
                    State = "PendingConfirmation",
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    EntityType = "EmailLoginTokenEntity",
                    PartitionKey = "#test-email".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin-confirmationToken".ToEncodedKeyProperty(),
                    Token = "token",
                    Created = DateTime.UtcNow
                })
            });

            var command = new CreateUserSessionCommand
            {
                Email = "#test-eMail",
                Password = "#test-password"
            };

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _createUserSessionCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenPasswordsDoNotMatch_ThrowsException()
        {
            _cryptographicHashService
                .Setup(cryptographicHashService => cryptographicHashService.GetHash("#test-salt#test-password"))
                .Returns("#test-hash-bad")
                .Verifiable();
            _entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "#test-email".ToEncodedKeyProperty(),
                RowKey = "EmailLogin".ToEncodedKeyProperty(),
                PasswordSalt = "#test-salt",
                PasswordHash = "#test-hash",
                State = "Confirmed"
            }));

            var command = new CreateUserSessionCommand
            {
                Email = "#test-eMail",
                Password = "#test-password"
            };

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _createUserSessionCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }
    }
}