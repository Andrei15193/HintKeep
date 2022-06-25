using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HintKeep.Requests.Accounts.Queries;
using HintKeep.RequestsHandlers.Accounts.Queries;
using HintKeep.Storage;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using HintKeep.Tests.Stubs;
using HintKeep.ViewModels.Accounts;
using MediatR;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Accounts.Queries
{
    public class GetDeletedAccountsQueryHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<GetDeletedAccountsQuery, IReadOnlyList<AccountSummary>> _getAccountsQueryHandler;

        public GetDeletedAccountsQueryHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _entityTables.AccountHints.Create();
            _getAccountsQueryHandler = new GetDeletedAccountsQueryHandler(_entityTables, new Session("#user-id"));
        }

        [Fact]
        public async Task Handle_ExistingAccounts_ReturnsOnlyOwnUserDeletedAccounts()
        {
            var account = new Account
            {
                UserId = "#user-id",
                IsDeleted = true
            };
            _entityTables.AddAccounts(account, new Account { UserId = "other-user-id" });

            var accounts = await _getAccountsQueryHandler.Handle(new GetDeletedAccountsQuery(), default);

            Assert.Equal(
                new[]
                {
                    new
                    {
                        account.Id,
                        account.Name,
                        account.Hints.Single().Hint,
                        account.IsPinned
                    }
                },
                accounts
                    .Select(account => new
                    {
                        account.Id,
                        account.Name,
                        account.Hint,
                        account.IsPinned
                    })
                    .ToArray()
            );
        }

        [Fact]
        public async Task Handle_ExistingAccounts_ReturnsAllAccountsSortedByIsPinnedThenByName()
        {
            var expectedAccounts = new[]
            {
                new Account
                {
                    UserId = "#user-id",
                    Name = "B",
                    IsPinned = false,
                    IsDeleted = true
                },
                new Account
                {
                    UserId = "#user-id",
                    Name = "A",
                    IsPinned = false,
                    IsDeleted = true
                },
                new Account
                {
                    UserId = "#user-id",
                    Name = "BB",
                    IsPinned = true,
                    IsDeleted = true
                },
                new Account
                {
                    UserId = "#user-id",
                    Name = "AA",
                    IsPinned = true,
                    IsDeleted = true
                }
            };
            _entityTables.AddAccounts(expectedAccounts);

            var actualAccounts = await _getAccountsQueryHandler.Handle(new GetDeletedAccountsQuery(), default);

            Assert.Equal(
                expectedAccounts
                    .OrderByDescending(account => account.IsPinned)
                    .ThenBy(account => account.Name)
                    .Select(account => new
                    {
                        account.Id,
                        account.Name,
                        account.Hints.Single().Hint,
                        account.IsPinned
                    })
                    .ToArray(),
                actualAccounts
                    .Select(accountResult => new
                    {
                        accountResult.Id,
                        accountResult.Name,
                        accountResult.Hint,
                        accountResult.IsPinned
                    })
                    .ToArray()
            );
        }

        [Fact]
        public async Task Handle_WithDeletedAccounts_ReturnsOnlyDeletedAccounts()
        {
            var account = new Account { UserId = "#user-id", IsDeleted = true };
            _entityTables.AddAccounts(account, new Account { UserId = "#user-id", Name = "#Test-Name-Deleted" });

            var accounts = await _getAccountsQueryHandler.Handle(new GetDeletedAccountsQuery(), default);

            Assert.Equal(
                new[]
                {
                    new
                    {
                        account.Id,
                        account.Name,
                        account.Hints.Single().Hint,
                        account.IsPinned
                    }
                },
                accounts
                    .Select(account => new
                    {
                        account.Id,
                        account.Name,
                        account.Hint,
                        account.IsPinned
                    })
                    .ToArray()
            );
        }
    }
}