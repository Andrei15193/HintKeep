using System;
using System.Linq;
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
    public class DeleteAccountCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<DeleteAccountCommand> _moveAccountToBinCommandHandler;

        public DeleteAccountCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _moveAccountToBinCommandHandler = new DeleteAccountCommandHandler(_entityTables, new Session("#user-id"));
        }

        [Fact]
        public async Task Handle_WhenAccountDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _moveAccountToBinCommandHandler.Handle(new DeleteAccountCommand("#account-id"), default));
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenAccountExistAndIsDeleted_DeletesItFromDatabase()
        {
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = Array.Empty<AccountHint>(),
                IsDeleted = true
            };
            _entityTables.AddAccounts(account);

            await _moveAccountToBinCommandHandler.Handle(new DeleteAccountCommand("#account-id"), default);

            _entityTables.AssertAccounts(Enumerable.Empty<Account>());
        }

        [Fact]
        public async Task Handle_WhenAccountExistsAndIsNotDeleted_ThrowsException()
        {
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                IsDeleted = false
            };
            _entityTables.AddAccounts(account);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _moveAccountToBinCommandHandler.Handle(new DeleteAccountCommand("#account-id"), default));
            Assert.Empty(exception.Message);
            _entityTables.AssertAccounts(account);
        }
    }
}