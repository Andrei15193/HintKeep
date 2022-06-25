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
    public class UpdateDeletedAccountCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<UpdateDeletedAccountCommand> _updateAccountCommandHandler;

        public UpdateDeletedAccountCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _entityTables.AccountHints.Create();
            _updateAccountCommandHandler = new UpdateDeletedAccountCommandHandler(_entityTables, new Session("#user-id"));
        }

        [Fact]
        public async Task Handle_NonExistingAccount_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _updateAccountCommandHandler.Handle(
                    new UpdateDeletedAccountCommand(
                        "#account-id",
                        IsDeleted: false
                    ),
                    default
                )
            );
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_ExistingDeletedAccount_UpdatesIsDeletedFlag()
        {
            var account = new Account
            {
                UserId = "#user-id",
                IsDeleted = true
            };
            _entityTables.AddAccounts(account);

            await _updateAccountCommandHandler.Handle(
                new UpdateDeletedAccountCommand(
                    account.Id,
                    IsDeleted: false
                ),
                default
            );

            _entityTables.AssertAccounts(new Account(account) { IsDeleted = false });
        }

        [Fact]
        public async Task Handle_ExistingNotDeletedAccount_ThrowsException()
        {
            var account = new Account
            {
                UserId = "#user-id",
                IsDeleted = false
            };
            _entityTables.AddAccounts(account);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _updateAccountCommandHandler.Handle(
                new UpdateDeletedAccountCommand(
                    account.Id,
                    IsDeleted: false
                ),
                default
            ));
            Assert.Empty(exception.Message);
            _entityTables.AssertAccounts(account);
        }
    }
}