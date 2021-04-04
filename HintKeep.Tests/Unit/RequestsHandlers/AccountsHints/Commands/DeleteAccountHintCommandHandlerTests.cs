using System;
using System.Linq;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.AccountsHints.Commands;
using HintKeep.RequestsHandlers.AccountsHints.Commands;
using HintKeep.Storage;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using HintKeep.Tests.Stubs;
using MediatR;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.AccountsHints.Commands
{
    public class DeleteAccountHintCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<DeleteAccountHintCommand> _deleteAccountHintCommandHandler;

        public DeleteAccountHintCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _deleteAccountHintCommandHandler = new DeleteAccountHintCommandHandler(_entityTables, new Session("#user-id"));
        }

        [Fact]
        public async Task Handle_WhenReferredHintIsNotLatest_DeletesItWithoutUpdatingAccount()
        {
            var now = DateTime.UtcNow;
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id-1",
                        Hint = "#Test-Hint-1",
                        DateAdded = now
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        Hint = "#Test-Hint-2",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _deleteAccountHintCommandHandler.Handle(new DeleteAccountHintCommand { AccountId = "#account-id", HintId = "#hint-id-2" }, default);
            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    account.Hints.First()
                }
            });
        }

        [Fact]
        public async Task Handle_WhenReferredHintIsLatest_DeletesItAndUpdatesAccount()
        {
            var now = DateTime.UtcNow;
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id-1",
                        Hint = "#Test-Hint-1",
                        DateAdded = now
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        Hint = "#Test-Hint-2",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _deleteAccountHintCommandHandler.Handle(new DeleteAccountHintCommand { AccountId = "#account-id", HintId = "#hint-id-1" }, default);
            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    account.Hints.Last()
                }
            });
        }

        [Fact]
        public async Task Handle_WhenThereIsOnlyOneHint_DeletesItAndUpdatesAccount()
        {
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id",
                        Hint = "#Test-Hint",
                        DateAdded = DateTime.UtcNow
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _deleteAccountHintCommandHandler.Handle(new DeleteAccountHintCommand { AccountId = "#account-id", HintId = "#hint-id" }, default);
            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = Array.Empty<AccountHint>()
            });
        }

        [Fact]
        public async Task Handle_WhenHintDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _deleteAccountHintCommandHandler.Handle(new DeleteAccountHintCommand { AccountId = "#account-id", HintId = "#hint-id" }, default));
            Assert.Empty(exception.Message);
        }
    }
}