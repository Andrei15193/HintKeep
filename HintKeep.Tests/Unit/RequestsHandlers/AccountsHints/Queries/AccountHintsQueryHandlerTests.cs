using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.AccountsHints.Queries;
using HintKeep.RequestsHandlers.AccountsHints.Queries;
using HintKeep.Storage;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using HintKeep.Tests.Stubs;
using HintKeep.ViewModels.AccountsHints;
using MediatR;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.AccountsHints.Queries
{
    public class AccountHintsQueryHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<AccountHintsQuery, IEnumerable<AccountHintDetails>> _accountHintsQueryHandler;

        public AccountHintsQueryHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _entityTables.AccountHints.Create();
            _accountHintsQueryHandler = new AccountHintsQueryHandler(_entityTables, new Session("#user-id"));
        }

        [Fact]
        public async Task Handle_WhenAccountHasHints_ReturnsThemSortedByDateAddedThenByName()
        {
            var now = DateTime.UtcNow;
            var accounts = new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id-5",
                        DateAdded = null,
                        Hint = "#Test-Hint-5"
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-2",
                        DateAdded = now,
                        Hint = "#Test-Hint-2"
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-1",
                        DateAdded = now,
                        Hint = "#Test-Hint-1"
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-3",
                        DateAdded = now,
                        Hint = "#Test-Hint-3"
                    },
                    new AccountHint
                    {
                        Id = "#hint-id-4",
                        DateAdded = now.AddDays(-1),
                        Hint = "#Test-Hint-4"
                    }
                }
            };
            _entityTables.AddAccounts(accounts);

            var accountHints = await _accountHintsQueryHandler.Handle(new AccountHintsQuery("#account-id"), default);

            Assert.Equal(
                new[]
                {
                    new
                    {
                        Id = "#hint-id-1",
                        Hint = "#Test-Hint-1",
                        DateAdded = (DateTime?)now
                    },
                    new
                    {
                        Id = "#hint-id-2",
                        Hint = "#Test-Hint-2",
                        DateAdded = (DateTime?)now
                    },
                    new
                    {
                        Id = "#hint-id-3",
                        Hint = "#Test-Hint-3",
                        DateAdded = (DateTime?)now
                    },
                    new
                    {
                        Id = "#hint-id-4",
                        Hint = "#Test-Hint-4",
                        DateAdded = (DateTime?)now.AddDays(-1)
                    },
                    new
                    {
                        Id = "#hint-id-5",
                        Hint = "#Test-Hint-5",
                        DateAdded = default(DateTime?)
                    }
                },
                accountHints
                    .Select(accountHint => new
                    {
                        accountHint.Id,
                        accountHint.Hint,
                        accountHint.DateAdded
                    })
                    .ToArray()
            );
        }

        [Fact]
        public async Task Handle_WhenAccountDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _accountHintsQueryHandler.Handle(new AccountHintsQuery("#account-id"), default));
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
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _accountHintsQueryHandler.Handle(new AccountHintsQuery("#account-id"), default));
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenThereAreMultipleAccounts_ReturnsHintsForQueriedAccountOnly()
        {
            var now = DateTime.UtcNow;
            _entityTables.AddAccounts(new Account
            {
                UserId = "#user-id",
                Id = "#account-id",
                Name = "#account-name",
                Hints = new[]
                {
                    new AccountHint
                    {
                        Id = "#hint-id",
                        Hint = "#hint",
                        DateAdded = now
                    }
                }
            });
            _entityTables.AddAccounts(Enumerable.Range(1, 100).Select(accountNumber => new Account { UserId = "#user-id", Id = $"#account-id-{accountNumber}", Name = $"#account-name-{accountNumber}" }).ToArray());

            var accountHints = await _accountHintsQueryHandler.Handle(new AccountHintsQuery("#account-id"), default);

            Assert.Equal(
                new[]
                {
                    new
                    {
                        Id = "#hint-id",
                        Hint = "#hint",
                        DateAdded = (DateTime?)now
                    }
                },
                accountHints
                    .Select(accountHint => new
                    {
                        accountHint.Id,
                        accountHint.Hint,
                        accountHint.DateAdded
                    })
                    .ToArray()
            );
        }
    }
}