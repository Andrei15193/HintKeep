using System;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Commands;
using HintKeep.RequestsHandlers.Users.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Stubs;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Users.Commands
{
    public class DeleteCurrentUserSessionCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<DeleteCurrentUserSessionCommand> _deleteCurrentUserSessionCommandHandler;

        public DeleteCurrentUserSessionCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Users.Create();
            _entityTables.UserSessions.Create();
            _deleteCurrentUserSessionCommandHandler = new DeleteCurrentUserSessionCommandHandler(_entityTables, new Session("#user-id", "#session-id"));
        }

        [Fact]
        public async Task Handle_WhenThereIsAnActiveSession_DeletesTheSession()
        {
            _entityTables.UserSessions.Execute(TableOperation.Insert(new UserSessionEntity
            {
                EntityType = "UserSessionEntity",
                PartitionKey = "#user-id".ToEncodedKeyProperty(),
                RowKey = "#session-id".ToEncodedKeyProperty(),
                Expiration = DateTime.UtcNow.AddHours(1)
            }));

            await _deleteCurrentUserSessionCommandHandler.Handle(new DeleteCurrentUserSessionCommand(), CancellationToken.None);

            Assert.Empty(_entityTables.UserSessions.ExecuteQuery(new TableQuery()));
        }

        [Fact]
        public async Task Handle_WhenThereIsNoActiveSession_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _deleteCurrentUserSessionCommandHandler.Handle(new DeleteCurrentUserSessionCommand(), CancellationToken.None));
            Assert.Empty(exception.Message);
        }
    }
}