using System;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Accounts.Commands;
using HintKeep.RequestsHandlers.Accounts.Commands;
using HintKeep.Storage;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using HintKeep.Tests.Stubs;
using MediatR;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Accounts.Commands
{
    public class MoveAccountToBinCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<MoveAccountToBinCommand> _moveAccountToBinCommandHandler;

        public MoveAccountToBinCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _moveAccountToBinCommandHandler = new MoveAccountToBinCommandHandler(_entityTables, new Session("#user-id", "#session-id"));
        }

        [Fact]
        public async Task Handle_WhenAccountDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _moveAccountToBinCommandHandler.Handle(new MoveAccountToBinCommand { Id = "#account-id" }, CancellationToken.None));
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenAccountExist_MarksItAsDeleted()
        {
            var account = new Account
            {
                UserId = "#user-id"
            };
            _entityTables.AddAccounts(account);

            await _moveAccountToBinCommandHandler.Handle(new MoveAccountToBinCommand { Id = account.Id }, CancellationToken.None);

            _entityTables.AssertAccounts(new Account(account) { IsDeleted = true });
        }

        [Fact]
        public async Task Handle_WhenAccountIsDeleted_ThrowsException()
        {
            var account = new Account
            {
                UserId = "#user-id",
                IsDeleted = true
            };
            _entityTables.AddAccounts(account);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _moveAccountToBinCommandHandler.Handle(new MoveAccountToBinCommand { Id = "#account-id" }, CancellationToken.None));
            Assert.Empty(exception.Message);
            _entityTables.AssertAccounts(account);
        }
    }
}