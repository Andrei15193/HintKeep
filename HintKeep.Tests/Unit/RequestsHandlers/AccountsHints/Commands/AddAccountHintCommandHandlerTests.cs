using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.AccountsHints.Commands;
using HintKeep.RequestsHandlers.AccountsHints.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using HintKeep.Tests.Stubs;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.AccountsHints.Commands
{
    public class AddAccountHintCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<AddAccountHintCommand, string> _addAccountHintCommandHandler;

        public AddAccountHintCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _addAccountHintCommandHandler = new AddAccountHintCommandHandler(_entityTables, new Session("#user-id", "#session-id"));
        }

        [Fact]
        public async Task Handle_WhenProvingNewHintToAccountWithoutHints_UpdatesHintOnAccount()
        {
            var now = DateTime.UtcNow;
            var account = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = Array.Empty<AccountHint>()
            };
            _entityTables.AddAccounts(account);

            await _addAccountHintCommandHandler.Handle(
                new AddAccountHintCommand
                {
                    AccountId = "#account-id",
                    Hint = "#hint",
                    DateAdded = now
                },
                CancellationToken.None
            );

            var accountHint = Assert.Single(_entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity"))));
            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = accountHint.HintId,
                        Hint = "#hint",
                        DateAdded = now
                    }
                }
            });
        }

        [Fact]
        public async Task Handle_WhenProvingNewHint_UpdatesLatestHintOnAccount()
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
                        Id = "#hint-id",
                        Hint = "#hint-existing",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _addAccountHintCommandHandler.Handle(
                new AddAccountHintCommand
                {
                    AccountId = "#account-id",
                    Hint = "#hint-new",
                    DateAdded = now
                },
                CancellationToken.None
            );

            var accountHint = Assert.Single(
                _entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity"))),
                accountHint => accountHint.DateAdded == now
            );
            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = accountHint.HintId,
                        Hint = "#hint-new",
                        DateAdded = now
                    },
                    account.Hints.Single()
                }
            });
        }

        [Fact]
        public async Task Handle_WhenProvingNewHintWithoutDateAdded_DoesNotUpdateAccountHint()
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
                        Hint = "#hint-existing",
                        DateAdded = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _addAccountHintCommandHandler.Handle(
                new AddAccountHintCommand
                {
                    AccountId = "#account-id",
                    Hint = "#hint-new"
                },
                CancellationToken.None
            );

            var accountHint = Assert.Single(
                _entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity"))),
                accountHint => accountHint.DateAdded is null
            );
            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    account.Hints.Single(),
                    new AccountHint
                    {
                        Id = accountHint.HintId,
                        Hint = "#hint-new",
                        DateAdded = null
                    }
                }
            });
        }

        [Fact]
        public async Task Handle_WhenProvingNewHintWithDateEarlierThanLatest_DoesNotUpdateAccountHint()
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
                        Id = "#hint-id",
                        Hint = "#hint-existing",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _addAccountHintCommandHandler.Handle(
                new AddAccountHintCommand
                {
                    AccountId = "#account-id",
                    Hint = "#hint-new",
                    DateAdded = now.AddDays(-2)
                },
                CancellationToken.None
            );

            var accountHint = Assert.Single(
                _entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity"))),
                accountHint => accountHint.DateAdded == now.AddDays(-2)
            );
            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    account.Hints.Single(),
                    new AccountHint
                    {
                        Id = accountHint.HintId,
                        Hint = "#hint-new",
                        DateAdded = now.AddDays(-2)
                    }
                }
            });
        }

        [Fact]
        public async Task Handle_WhenAccountDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _addAccountHintCommandHandler.Handle(
                    new AddAccountHintCommand
                    {
                        AccountId = "#account-id",
                        Hint = "#hint-new"
                    },
                    CancellationToken.None
                )
            );
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenAccountIsDeleted_ThrowsException()
        {
            _entityTables.AddAccounts(new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                IsDeleted = true
            });

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _addAccountHintCommandHandler.Handle(
                    new AddAccountHintCommand
                    {
                        AccountId = "#account-id",
                        Hint = "#hint-new"
                    },
                    CancellationToken.None
                )
            );
            Assert.Empty(exception.Message);
        }
    }
}