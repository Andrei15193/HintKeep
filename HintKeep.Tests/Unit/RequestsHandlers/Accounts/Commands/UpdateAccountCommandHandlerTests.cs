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
    public class UpdateAccountCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<UpdateAccountCommand> _updateAccountCommandHandler;

        public UpdateAccountCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _updateAccountCommandHandler = new UpdateAccountCommandHandler(_entityTables, new Session("#user-id", "#session-id"));
        }

        [Fact]
        public async Task Handle_NonExistingAccount_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _updateAccountCommandHandler.Handle(
                    new UpdateAccountCommand
                    {
                        Id = "#account-id",
                        Name = "#Test-Name-Updated",
                        Hint = "#test-hint",
                        IsPinned = true
                    },
                    CancellationToken.None
                )
            );
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_ExistingAccount_UpdatesName()
        {
            var account = new Account
            {
                UserId = "#user-id"
            };
            _entityTables.AddAccounts(account);

            await _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = account.Id,
                    Name = "#Test-Name-Updated",
                    Hint = account.Hints.Single().Hint,
                    Notes = account.Notes,
                    IsPinned = account.IsPinned
                },
                CancellationToken.None
            );

            _entityTables.AssertAccounts(new Account(account) { Name = "#Test-Name-Updated" });
        }

        [Fact]
        public async Task Handle_ExistingAccount_UpdatesHint()
        {
            var account = new Account
            {
                UserId = "#user-id"
            };
            _entityTables.AddAccounts(account);

            await _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = account.Id,
                    Name = account.Name,
                    Hint = "#Test-Hint-Updated",
                    Notes = account.Notes,
                    IsPinned = account.IsPinned
                },
                CancellationToken.None
            );

            var latestAccountHintEntity = _entityTables
                .Accounts
                .ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(AccountHintEntity.AccountId), QueryComparisons.Equal, account.Id)))
                .Where(accountHintEntity => accountHintEntity.HintId != account.Hints.Single().Id)
                .Single();
            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = latestAccountHintEntity.HintId,
                        Hint = "#Test-Hint-Updated",
                        DateAdded = latestAccountHintEntity.DateAdded.Value
                    },
                    new AccountHint(account.Hints.Single())
                }
            });
        }

        [Fact]
        public async Task Handle_ExistingAccount_UpdatesNotes()
        {
            var account = new Account
            {
                UserId = "#user-id"
            };
            _entityTables.AddAccounts(account);

            await _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = account.Id,
                    Name = account.Name,
                    Hint = account.Hints.Single().Hint,
                    Notes = "#Test-Notes-Updated",
                    IsPinned = account.IsPinned
                },
                CancellationToken.None
            );

            _entityTables.AssertAccounts(new Account(account) { Notes = "#Test-Notes-Updated" });
        }

        [Fact]
        public async Task Handle_ExistingAccount_UpdatesIsPinned()
        {
            var account = new Account
            {
                UserId = "#user-id"
            };
            _entityTables.AddAccounts(account);

            await _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = account.Id,
                    Name = account.Name,
                    Hint = account.Hints.Single().Hint,
                    Notes = account.Notes,
                    IsPinned = false
                },
                CancellationToken.None
            );

            _entityTables.AssertAccounts(new Account(account) { IsPinned = false });
        }

        [Fact]
        public async Task Handle_ExistingDeletedAccount_ThrowsException()
        {
            var account = new Account
            {
                UserId = "#user-id",
                IsDeleted = true
            };
            _entityTables.AddAccounts(account);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = account.Id,
                    Name = account.Name,
                    Hint = account.Hints.Single().Hint,
                    IsPinned = account.IsPinned
                },
                CancellationToken.None
            ));
            Assert.Empty(exception.Message);
            _entityTables.AssertAccounts(account);
        }
    }
}