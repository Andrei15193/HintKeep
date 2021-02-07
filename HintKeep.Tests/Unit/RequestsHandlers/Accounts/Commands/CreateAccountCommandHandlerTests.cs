using System;
using System.Linq;
using System.Threading;
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
    public class CreateAccountCommandHandlerTests
    {
        private readonly string _userId;
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<CreateAccountCommand, string> _createAccountCommandHandler;

        public CreateAccountCommandHandlerTests()
        {
            _userId = Guid.NewGuid().ToString("N");
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _createAccountCommandHandler = new CreateAccountCommandHandler(_entityTables, new LoginInfo(_userId));
        }

        [Fact]
        public async Task Handle_NewAccount_InsertsAccountEntity()
        {
            var accountId = await _createAccountCommandHandler.Handle(
                new CreateAccountCommand
                {
                    Name = "Test-Name",
                    Hint = "Test-Hint",
                    Notes = "Test-Notes",
                    IsPinned = true
                },
                CancellationToken.None
            );

            Assert.Equal(3, _entityTables.Accounts.ExecuteQuery(new TableQuery<AccountEntity>()).Count());
            var accountHintEntity = Assert.Single(_entityTables.Accounts.ExecuteQuery(
                new TableQuery<AccountHintEntity>()
                    .Where(TableQuery.GenerateFilterCondition(nameof(AccountHintEntity.AccountId), QueryComparisons.Equal, accountId))
            ));
            _entityTables.AssertAccounts(new Account
            {
                UserId = _userId,
                Id = accountId,
                Name = "Test-Name",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Hint ="Test-Hint",
                        StartDate = accountHintEntity.StartDate.Value
                    }
                },
                Notes = "Test-Notes",
                IsPinned = true
            });
        }

        [Fact]
        public async Task Handle_ExistingAccount_ThrowsExceptuon()
        {
            var account = new Account
            {
                UserId = _userId
            };
            _entityTables.AddAccounts(account);

            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _createAccountCommandHandler.Handle(
                    new CreateAccountCommand
                    {
                        Name = account.Name,
                        Hint = "Test-Hint"
                    },
                    CancellationToken.None
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
                UserId = _userId,
                IsDeleted = true
            };
            _entityTables.AddAccounts(account);

            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _createAccountCommandHandler.Handle(
                    new CreateAccountCommand
                    {
                        Name = "Test-Name",
                        Hint = "Test-Hint-2"
                    },
                    CancellationToken.None
                )
            );
            Assert.Empty(exception.Message);
            _entityTables.AssertAccounts(account);
        }
    }
}