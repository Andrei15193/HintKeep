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
    public class UpdateAccountHintCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<UpdateAccountHintCommand> _updateAccountHintCommandHandler;

        public UpdateAccountHintCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _entityTables.AccountHints.Create();
            _updateAccountHintCommandHandler = new UpdateAccountHintCommandHandler(_entityTables, new Session("#user-id"));
        }

        [Fact]
        public async Task Handle_WhenAccountHintHasDateSet_UpdatesTheHintDate()
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
                        DateAdded = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _updateAccountHintCommandHandler.Handle(
                new UpdateAccountHintCommand(
                    "#account-id",
                    "#hint-id",
                    DateAdded: now
                ),
                default
            );

            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint(account.Hints.Single())
                    {
                        DateAdded = now
                    }
                }
            });
        }

        [Fact]
        public async Task Handle_WhenAccountHintDoesNotHaveDateSet_UpdatesTheHintDate()
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
                        DateAdded = null
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _updateAccountHintCommandHandler.Handle(
                new UpdateAccountHintCommand(
                    "#account-id",
                    "#hint-id",
                    DateAdded: now
                ),
                default
            );

            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint(account.Hints.Single())
                    {
                        DateAdded = now
                    }
                }
            });
        }

        [Fact]
        public async Task Handle_WhenAccountHintIsOlderThanCurrentOneButUpdatedAsLatest_UpdatesTheAccountHint()
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
                        DateAdded = now.AddDays(-1)
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        DateAdded = now.AddDays(-2)
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _updateAccountHintCommandHandler.Handle(
                new UpdateAccountHintCommand(
                    "#account-id",
                    "#hint-id-2",
                    DateAdded: now
                ),
                default
            );

            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint(account.Hints.First()),
                    new AccountHint(account.Hints.Last())
                    {
                        DateAdded = now
                    }
                }
            });
        }

        [Fact]
        public async Task Handle_WhenAccountCurrentHintDateAddedIsSetToOldest_UpdatesThePreviousAccountHintAsCurrent()
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
                        DateAdded = now
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _updateAccountHintCommandHandler.Handle(
                new UpdateAccountHintCommand(
                    "#account-id",
                    "#hint-id-1",
                    DateAdded: now.AddDays(-2)
                ),
                default
            );

            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint(account.Hints.First())
                    {
                        DateAdded = now.AddDays(-2)
                    },
                    account.Hints.Last()
                }
            });
        }

        [Fact]
        public async Task Handle_WhenAccountCurrentHintDateAddedIsRemoved_UpdatesThePreviousAccountHintAsCurrent()
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
                        DateAdded = now
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        DateAdded = now.AddDays(-1)
                    }
                }
            };
            _entityTables.AddAccounts(account);

            await _updateAccountHintCommandHandler.Handle(
                new UpdateAccountHintCommand(
                    "#account-id",
                    "#hint-id-1",
                    DateAdded: null
                ),
                default
            );

            _entityTables.AssertAccounts(new Account(account)
            {
                Hints = new[]
                {
                    new AccountHint(account.Hints.First())
                    {
                        DateAdded = null
                    },
                    account.Hints.Last()
                }
            });
        }

        [Fact]
        public async Task Handle_WhenAccountDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _updateAccountHintCommandHandler.Handle(
                    new UpdateAccountHintCommand(
                        "#account-id",
                        "#hint-id",
                        DateAdded: null
                    ),
                    default
                )
            );
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenAccountHintDoesNotExist_ThrowsException()
        {
            _entityTables.AddAccounts(new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id"
                    }
                }
            });
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _updateAccountHintCommandHandler.Handle(
                    new UpdateAccountHintCommand(
                        "#account-id",
                        "#hint-id-not-existing",
                        DateAdded: null
                    ),
                    default
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
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id"
                    }
                },
                IsDeleted = true
            });

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _updateAccountHintCommandHandler.Handle(
                    new UpdateAccountHintCommand(
                        "#account-id",
                        "#hint-id",
                        DateAdded: null
                    ),
                    default
                )
            );
            Assert.Empty(exception.Message);
        }
    }
}