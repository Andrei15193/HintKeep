using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Requests.Accounts.Queries;
using HintKeep.RequestsHandlers.Accounts.Queries;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Data;
using HintKeep.Tests.Data.Extensions;
using HintKeep.Tests.Stubs;
using HintKeep.ViewModels.Accounts;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Accounts.Queries
{
    public class GetAccountsQueryHandlerTests
    {
        private readonly string _userId;
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<GetAccountsQuery, IReadOnlyList<AccountSummary>> _getAccountsQueryHandler;

        public GetAccountsQueryHandlerTests()
        {
            _userId = Guid.NewGuid().ToString("N");
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _getAccountsQueryHandler = new GetAccountsQueryHandler(_entityTables, new LoginInfo(_userId));
        }

        [Fact]
        public async Task Handle_ExistingAccounts_ReturnsOnlyOwnUserAccounts()
        {
            var account = new Account
            {
                UserId = _userId
            };
            _entityTables.AddAccounts(account, new Account { UserId = "other-user-id" });

            var accounts = await _getAccountsQueryHandler.Handle(new GetAccountsQuery(), CancellationToken.None);

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
                    UserId = _userId,
                    Name = "B",
                    IsPinned = false
                },
                new Account
                {
                    UserId = _userId,
                    Name = "A",
                    IsPinned = false
                },
                new Account
                {
                    UserId = _userId,
                    Name = "BB",
                    IsPinned = true
                },
                new Account
                {
                    UserId = _userId,
                    Name = "AA",
                    IsPinned = true
                }
            };
            _entityTables.AddAccounts(expectedAccounts);

            var actualAccounts = await _getAccountsQueryHandler.Handle(new GetAccountsQuery(), CancellationToken.None);

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
        public async Task Handle_WithDeletedAccounts_ReturnsOnlyNonDeletedAccounts()
        {
            var account = new Account { UserId = _userId };
            _entityTables.AddAccounts(account, new Account { UserId = _userId, Name = "#Test-Name-Deleted", IsDeleted = true });

            var accounts = await _getAccountsQueryHandler.Handle(new GetAccountsQuery(), CancellationToken.None);

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