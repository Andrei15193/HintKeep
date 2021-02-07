using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Accounts.Commands;
using HintKeep.RequestsHandlers.Accounts.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
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
            _updateAccountCommandHandler = new UpdateAccountCommandHandler(_entityTables, new LoginInfo("user-id"));
        }

        [Fact]
        public async Task Handle_NonExistingAccount_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _updateAccountCommandHandler.Handle(
                    new UpdateAccountCommand
                    {
                        Id = "account-id",
                        Name = "test-name-updated",
                        Hint = "test-hint",
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
            var now = DateTime.UtcNow;
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = "user-id",
                    RowKey = "name-test-name",
                    IndexedEntityId = "account-id"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = "user-id",
                    RowKey = "id-account-id",
                    Id = "account-id",
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = true,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = "user-id",
                    RowKey = $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    AccountId = "account-id",
                    Hint = "test-hint",
                    StartDate = now
                })
            });

            await _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = "account-id",
                    Name = "test-name-updated",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = true
                },
                CancellationToken.None
            );

            Assert.Equal(3, _entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexEntity = (IndexEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>("user-id", "name-test-name-updated")).Result;
            Assert.Equal("IndexEntity", indexEntity.EntityType);
            Assert.Equal("user-id", indexEntity.PartitionKey);
            Assert.Equal("name-test-name-updated", indexEntity.RowKey);
            Assert.Equal("account-id", indexEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("user-id", "id-account-id")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("user-id", accountEntity.PartitionKey);
            Assert.Equal("id-account-id", accountEntity.RowKey);
            Assert.Equal("account-id", accountEntity.Id);
            Assert.Equal("test-name-updated", accountEntity.Name);
            Assert.Equal("test-hint", accountEntity.Hint);
            Assert.Equal("test-notes", accountEntity.Notes);
            Assert.True(accountEntity.IsPinned);
            Assert.False(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>("user-id", $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("user-id", accountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", accountHintEntity.RowKey);
            Assert.Equal("account-id", accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.StartDate);
        }

        [Fact]
        public async Task Handle_ExistingAccount_UpdatesHint()
        {
            var yestarday = DateTime.UtcNow.AddDays(-1);
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = "user-id",
                    RowKey = "name-test-name",
                    IndexedEntityId = "account-id"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = "user-id",
                    RowKey = "id-account-id",
                    Id = "account-id",
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = true,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = "user-id",
                    RowKey = $"id-account-id-hintDate-{yestarday:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    AccountId = "account-id",
                    Hint = "test-hint",
                    StartDate = yestarday
                })
            });

            await _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = "account-id",
                    Name = "test-name",
                    Hint = "test-hint-updated",
                    Notes = "test-notes",
                    IsPinned = true
                },
                CancellationToken.None
            );

            Assert.Equal(4, _entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexEntity = (IndexEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>("user-id", "name-test-name")).Result;
            Assert.Equal("IndexEntity", indexEntity.EntityType);
            Assert.Equal("user-id", indexEntity.PartitionKey);
            Assert.Equal("name-test-name", indexEntity.RowKey);
            Assert.Equal("account-id", indexEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("user-id", "id-account-id")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("user-id", accountEntity.PartitionKey);
            Assert.Equal("id-account-id", accountEntity.RowKey);
            Assert.Equal("account-id", accountEntity.Id);
            Assert.Equal("test-name", accountEntity.Name);
            Assert.Equal("test-hint-updated", accountEntity.Hint);
            Assert.Equal("test-notes", accountEntity.Notes);
            Assert.True(accountEntity.IsPinned);
            Assert.False(accountEntity.IsDeleted);

            var olderAccountHintEntity = (AccountHintEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>("user-id", $"id-account-id-hintDate-{yestarday:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}")).Result;
            Assert.Equal("AccountHintEntity", olderAccountHintEntity.EntityType);
            Assert.Equal("user-id", olderAccountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-{yestarday:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", olderAccountHintEntity.RowKey);
            Assert.Equal("account-id", olderAccountHintEntity.AccountId);
            Assert.Equal("test-hint", olderAccountHintEntity.Hint);
            Assert.Equal(yestarday, olderAccountHintEntity.StartDate);

            var newerAccountHintEntity = Assert.Single(_entityTables.Accounts.ExecuteQuery(new TableQuery<AccountHintEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.GreaterThan, $"id-account-id-hintDate-{yestarday:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")
                )
            )));
            Assert.Equal("AccountHintEntity", newerAccountHintEntity.EntityType);
            Assert.Equal("user-id", newerAccountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-{newerAccountHintEntity.StartDate:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", newerAccountHintEntity.RowKey);
            Assert.Equal("account-id", newerAccountHintEntity.AccountId);
            Assert.Equal("test-hint-updated", newerAccountHintEntity.Hint);
            Assert.True(DateTime.UtcNow.AddMinutes(-1) <= newerAccountHintEntity.StartDate && newerAccountHintEntity.StartDate <= DateTime.UtcNow.AddMinutes(1));
        }

        [Fact]
        public async Task Handle_ExistingAccount_UpdatesNotes()
        {
            var now = DateTime.UtcNow;
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = "user-id",
                    RowKey = "name-test-name",
                    IndexedEntityId = "account-id"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = "user-id",
                    RowKey = "id-account-id",
                    Id = "account-id",
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = true,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = "user-id",
                    RowKey = $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    AccountId = "account-id",
                    Hint = "test-hint",
                    StartDate = now
                })
            });

            await _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = "account-id",
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes-updated",
                    IsPinned = true
                },
                CancellationToken.None
            );

            Assert.Equal(3, _entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexEntity = (IndexEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>("user-id", "name-test-name")).Result;
            Assert.Equal("IndexEntity", indexEntity.EntityType);
            Assert.Equal("user-id", indexEntity.PartitionKey);
            Assert.Equal("name-test-name", indexEntity.RowKey);
            Assert.Equal("account-id", indexEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("user-id", "id-account-id")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("user-id", accountEntity.PartitionKey);
            Assert.Equal("id-account-id", accountEntity.RowKey);
            Assert.Equal("account-id", accountEntity.Id);
            Assert.Equal("test-name", accountEntity.Name);
            Assert.Equal("test-hint", accountEntity.Hint);
            Assert.Equal("test-notes-updated", accountEntity.Notes);
            Assert.True(accountEntity.IsPinned);
            Assert.False(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>("user-id", $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("user-id", accountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", accountHintEntity.RowKey);
            Assert.Equal("account-id", accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.StartDate);
        }

        [Fact]
        public async Task Handle_ExistingAccount_UpdatesIsPinned()
        {
            var now = DateTime.UtcNow;
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = "user-id",
                    RowKey = "name-test-name",
                    IndexedEntityId = "account-id"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = "user-id",
                    RowKey = "id-account-id",
                    Id = "account-id",
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = true,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = "user-id",
                    RowKey = $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    AccountId = "account-id",
                    Hint = "test-hint",
                    StartDate = now
                })
            });

            await _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = "account-id",
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = false
                },
                CancellationToken.None
            );

            Assert.Equal(3, _entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexEntity = (IndexEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>("user-id", "name-test-name")).Result;
            Assert.Equal("IndexEntity", indexEntity.EntityType);
            Assert.Equal("user-id", indexEntity.PartitionKey);
            Assert.Equal("name-test-name", indexEntity.RowKey);
            Assert.Equal("account-id", indexEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("user-id", "id-account-id")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("user-id", accountEntity.PartitionKey);
            Assert.Equal("id-account-id", accountEntity.RowKey);
            Assert.Equal("account-id", accountEntity.Id);
            Assert.Equal("test-name", accountEntity.Name);
            Assert.Equal("test-hint", accountEntity.Hint);
            Assert.Equal("test-notes", accountEntity.Notes);
            Assert.False(accountEntity.IsPinned);
            Assert.False(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>("user-id", $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("user-id", accountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", accountHintEntity.RowKey);
            Assert.Equal("account-id", accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.StartDate);
        }

        [Fact]
        public async Task Handle_ExistingDeletedAccount_ThrowsException()
        {
            var now = DateTime.UtcNow;
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = "user-id",
                    RowKey = "name-test-name",
                    IndexedEntityId = "account-id"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = "user-id",
                    RowKey = "id-account-id",
                    Id = "account-id",
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = true,
                    IsDeleted = true
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = "user-id",
                    RowKey = $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    AccountId = "account-id",
                    Hint = "test-hint",
                    StartDate = now
                })
            });

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _updateAccountCommandHandler.Handle(
                new UpdateAccountCommand
                {
                    Id = "account-id",
                    Name = "test-name-updated",
                    Hint = "test-hint-updated",
                    IsPinned = false
                },
                CancellationToken.None
            ));
            Assert.Empty(exception.Message);

            Assert.Equal(3, _entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexEntity = (IndexEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>("user-id", "name-test-name")).Result;
            Assert.Equal("IndexEntity", indexEntity.EntityType);
            Assert.Equal("user-id", indexEntity.PartitionKey);
            Assert.Equal("name-test-name", indexEntity.RowKey);
            Assert.Equal("account-id", indexEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>("user-id", "id-account-id")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal("user-id", accountEntity.PartitionKey);
            Assert.Equal("id-account-id", accountEntity.RowKey);
            Assert.Equal("account-id", accountEntity.Id);
            Assert.Equal("test-name", accountEntity.Name);
            Assert.Equal("test-hint", accountEntity.Hint);
            Assert.Equal("test-notes", accountEntity.Notes);
            Assert.True(accountEntity.IsPinned);
            Assert.True(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>("user-id", $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal("user-id", accountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", accountHintEntity.RowKey);
            Assert.Equal("account-id", accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.StartDate);
        }
    }
}