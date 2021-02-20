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
    public class RefreshUserSessionCommandHandlerTests : IDisposable
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;
        private readonly Mock<IJsonWebTokenService> _jsonWebTokenService;
        private readonly IRequestHandler<RefreshUserSessionCommand, UserSession> _refreshUserSessionCommandHandler;

        public RefreshUserSessionCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.UserSessions.Create();
            _session = new Session("#user-id", "#session-id");
            _jsonWebTokenService = new Mock<IJsonWebTokenService>();
            _refreshUserSessionCommandHandler = new RefreshUserSessionCommandHandler(_entityTables, _session, _jsonWebTokenService.Object);
        }

        public void Dispose()
            => _jsonWebTokenService.VerifyAll();

        [Fact]
        public async Task Handle_WhenSessionExists_ReturnsNewJwt()
        {
            _entityTables.UserSessions.Execute(TableOperation.InsertOrMerge(new UserSessionEntity
            {
                EntityType = "UserSessionEntity",
                PartitionKey = "#user-id".ToEncodedKeyProperty(),
                RowKey = "#session-id".ToEncodedKeyProperty(),
                Expiration = DateTime.UtcNow
            }));
            _jsonWebTokenService
                .Setup(jsonWebTokenService => jsonWebTokenService.GetJsonWebToken("#user-id", "#session-id"))
                .Returns("#jwt")
                .Verifiable();

            var userSession = await _refreshUserSessionCommandHandler.Handle(new RefreshUserSessionCommand(), default);

            Assert.Equal("#jwt", userSession.JsonWebToken);
            var userSessionEntity = Assert.Single(_entityTables.UserSessions.ExecuteQuery(new TableQuery<UserSessionEntity>()));
            Assert.Equal("UserSessionEntity", userSessionEntity.EntityType);
            Assert.Equal("#user-id", userSessionEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("#session-id", userSessionEntity.RowKey.FromEncodedKeyProperty());
            Assert.True(DateTime.UtcNow.AddHours(1).AddMinutes(-1) <= userSessionEntity.Expiration && userSessionEntity.Expiration <= DateTime.UtcNow.AddHours(1).AddMinutes(1));
        }

        [Fact]
        public async Task Handle_WhenSessionDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _refreshUserSessionCommandHandler.Handle(new RefreshUserSessionCommand(), default));
            Assert.Empty(exception.Message);
        }
    }
}