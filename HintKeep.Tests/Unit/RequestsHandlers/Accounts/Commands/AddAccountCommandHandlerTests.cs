using System.Linq;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Accounts.Commands;
using HintKeep.RequestsHandlers.Accounts.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using HintKeep.Tests.Stubs;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Accounts.Commands
{
    public class AddAccountCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<AddAccountCommand, string> _createAccountCommandHandler;

        public AddAccountCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _entityTables.AccountHints.Create();
            _createAccountCommandHandler = new AddAccountCommandHandler(_entityTables, new Session("#user-id"));
        }

        [Fact]
        public async Task Handle_NewAccount_InsertsAccountEntity()
        {
            var accountId = await _createAccountCommandHandler.Handle(
                new AddAccountCommand(
                    "#Test-Name",
                    "#Test-Hint",
                    "#Test-Notes",
                    IsPinned: true
                ),
                default
            );

            Assert.Equal(2, _entityTables.Accounts.ExecuteQuery(new TableQuery<AccountEntity>()).Count());
            var accountHintEntity = Assert.Single(_entityTables.AccountHints.ExecuteQuery(new TableQuery<AccountHintEntity>()));
            _entityTables.AssertAccounts(new Account
            {
                UserId = "#user-id",
                Id = accountId,
                Name = "#Test-Name",
                LatestHint = "#Test-Hint",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = accountHintEntity.HintId,
                        Hint ="#Test-Hint",
                        DateAdded = accountHintEntity.DateAdded.Value
                    }
                },
                Notes = "#Test-Notes",
                IsPinned = true
            });
        }

        [Fact]
        public async Task Handle_ExistingAccount_ThrowsExceptuon()
        {
            var account = new Account
            {
                UserId = "#user-id"
            };
            _entityTables.AddAccounts(account);

            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _createAccountCommandHandler.Handle(
                    new AddAccountCommand(
                        account.Name,
                        "#Test-Hint",
                        string.Empty,
                        IsPinned: false
                    ),
                    default
                )
            );
            Assert.Empty(exception.Message);
            _entityTables.AssertAccounts(account);
        }

        [Fact]
        public async Task Handle_ExistingDeletedAccount_ThrowsExceptuon()
        {
            var account = new Account
            {
                UserId = "#user-id",
                IsDeleted = true
            };
            _entityTables.AddAccounts(account);

            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _createAccountCommandHandler.Handle(
                    new AddAccountCommand(
                        "#Test-Name",
                        "#Test-Hint-2",
                        string.Empty,
                        IsPinned: false
                    ),
                    default
                )
            );
            Assert.Empty(exception.Message);
            _entityTables.AssertAccounts(account);
        }
    }
}