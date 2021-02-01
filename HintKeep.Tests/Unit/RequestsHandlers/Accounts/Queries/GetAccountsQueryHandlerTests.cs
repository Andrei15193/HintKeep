using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Requests.Accounts.Queries;
using HintKeep.RequestsHandlers.Accounts.Queries;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
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
        private readonly IRequestHandler<GetAccountsQuery, IReadOnlyList<Account>> _getAccountsQueryHandler;

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
            var otherUserId = Guid.NewGuid().ToString("N");
            while (otherUserId == _userId)
                otherUserId = Guid.NewGuid().ToString("N");
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    PartitionKey = _userId,
                    RowKey = "test-name",
                    IndexedEntityId = "1"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    PartitionKey = _userId,
                    RowKey = "id-1",
                    Id = "1",
                    Name = "test-name",
                    Hint = "test-hint",
                    IsPinned = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    PartitionKey = _userId,
                    RowKey = "id-1-hintDate-1",
                    AccountId = "1",
                    Hint = "test-hint"
                })
            });
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    PartitionKey = otherUserId,
                    RowKey = "name-A",
                    IndexedEntityId = "2"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    PartitionKey = otherUserId,
                    RowKey = "id-2",
                    Id = "2",
                    Name = "test-name",
                    Hint = "test-hint",
                    IsPinned = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    PartitionKey = otherUserId,
                    RowKey = "id-2-hintDate-2",
                    AccountId = "2",
                    Hint = "test-hint"
                })
            });

            var accounts = await _getAccountsQueryHandler.Handle(new GetAccountsQuery(), CancellationToken.None);

            var account = Assert.Single(accounts);
            Assert.Equal("1", account.Id);
            Assert.Equal("test-name", account.Name);
            Assert.Equal("test-hint", account.Hint);
            Assert.False(account.IsPinned);
        }

        [Fact]
        public async Task Handle_ExistingAccounts_ReturnsAllAccountsSortedByIsPinnedThenByName()
        {
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    PartitionKey = _userId,
                    RowKey = "name-B",
                    IndexedEntityId = "1"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    PartitionKey = _userId,
                    RowKey = "id-1",
                    Name = "B",
                    Hint = "test-hint",
                    IsPinned = false
                }),
                TableOperation.Insert(new IndexEntity
                {
                    PartitionKey = _userId,
                    RowKey = "name-A",
                    IndexedEntityId = "2"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    PartitionKey = _userId,
                    RowKey = "id-2",
                    Name = "A",
                    Hint = "test-hint",
                    IsPinned = false
                }),
                TableOperation.Insert(new IndexEntity
                {
                    PartitionKey = _userId,
                    RowKey = "name-BB",
                    IndexedEntityId = "3"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    PartitionKey = _userId,
                    RowKey = "id-3",
                    Name = "BB",
                    Hint = "test-hint",
                    IsPinned = true
                }),
                TableOperation.Insert(new IndexEntity
                {
                    PartitionKey = _userId,
                    RowKey = "name-AA",
                    IndexedEntityId = "4"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    PartitionKey = _userId,
                    RowKey = "id-4",
                    Name = "AA",
                    Hint = "test-hint",
                    IsPinned = true
                })
            });

            var accounts = await _getAccountsQueryHandler.Handle(new GetAccountsQuery(), CancellationToken.None);

            Assert.Equal(
                new[]
                {
                    new
                    {
                        Name = "AA",
                        IsPinned = true
                    },
                    new
                    {
                        Name = "BB",
                        IsPinned = true
                    },
                    new
                    {
                        Name = "A",
                        IsPinned = false
                    },
                    new
                    {
                        Name = "B",
                        IsPinned = false
                    }
                },
                accounts
                    .Select(account => new
                    {
                        account.Name,
                        account.IsPinned
                    })
                    .ToArray()
            );
        }
    }
}