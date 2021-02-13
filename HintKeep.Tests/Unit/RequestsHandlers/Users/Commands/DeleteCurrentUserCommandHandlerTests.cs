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
    public class DeleteCurrentUserCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;
        private readonly IRequestHandler<DeleteCurrentUserCommand> _deleteCurrentUserCommandHandler;

        public DeleteCurrentUserCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Users.Create();
            _session = new Session("#user-id", "#session-id");
            _deleteCurrentUserCommandHandler = new DeleteCurrentUserCommandHandler(_entityTables, _session);
        }

        [Fact]
        public async Task Handle_WhenUserExists_DeletesTheCurrentUser()
        {
            _entityTables.Users.Execute(TableOperation.Insert(new UserEntity
            {
                EntityType = "UserEntity",
                PartitionKey = "#user-id".ToEncodedKeyProperty(),
                RowKey = "details".ToEncodedKeyProperty()
            }));

            await _deleteCurrentUserCommandHandler.Handle(new DeleteCurrentUserCommand(), CancellationToken.None);

            Assert.Empty(_entityTables.Users.ExecuteQuery(new TableQuery()));
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _deleteCurrentUserCommandHandler.Handle(new DeleteCurrentUserCommand(), CancellationToken.None));
            Assert.Empty(exception.Message);
        }
    }
}