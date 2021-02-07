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
                    Name = "Test-Account",
                    Hint = "Test-Hint",
                    Notes = "Test-Notes",
                    IsPinned = true
                },
                CancellationToken.None
            );

            Assert.Equal(3, _entityTables.Accounts.ExecuteQuery(new TableQuery<AccountEntity>()).Count());

            var indexedEntity = (IndexEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(_userId, "name-test-account")).Result;
            Assert.Equal("IndexEntity", indexedEntity.EntityType);
            Assert.Equal(_userId, indexedEntity.PartitionKey);
            Assert.Equal("name-test-account", indexedEntity.RowKey);
            Assert.Equal(accountId, indexedEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(_userId, $"id-{accountId}")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal(_userId, accountEntity.PartitionKey);
            Assert.Equal($"id-{accountId}", accountEntity.RowKey);
            Assert.Equal("Test-Account", accountEntity.Name);
            Assert.Equal("Test-Hint", accountEntity.Hint);
            Assert.Equal("Test-Notes", accountEntity.Notes);
            Assert.True(accountEntity.IsPinned);
            Assert.False(accountEntity.IsDeleted);

            var accountHintEntity = Assert.Single(_entityTables.Accounts.ExecuteQuery(
                new TableQuery<AccountHintEntity>()
                    .Where(TableQuery.GenerateFilterCondition(nameof(AccountHintEntity.AccountId), QueryComparisons.NotEqual, string.Empty))
            ));
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal(_userId, accountHintEntity.PartitionKey);
            Assert.Equal($"id-{accountId}-hintDate-{accountHintEntity.StartDate:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", accountHintEntity.RowKey);
            Assert.NotNull(accountHintEntity.StartDate);
            Assert.Equal(accountEntity.Id, accountHintEntity.AccountId);
            Assert.Equal("Test-Hint", accountHintEntity.Hint);
        }

        [Fact]
        public async Task Handle_ExistingAccount_ThrowsExceptuon()
        {
            var now = DateTime.UtcNow;
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = _userId,
                    RowKey = "name-test-name",
                    IndexedEntityId = "account-id"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = _userId,
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
                    PartitionKey = _userId,
                    RowKey = $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    AccountId = "account-id",
                    Hint = "test-hint",
                    StartDate = now
                })
            });

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

            Assert.Equal(3, _entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexEntity = (IndexEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(_userId, "name-test-name")).Result;
            Assert.Equal("IndexEntity", indexEntity.EntityType);
            Assert.Equal(_userId, indexEntity.PartitionKey);
            Assert.Equal("name-test-name", indexEntity.RowKey);
            Assert.Equal("account-id", indexEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(_userId, "id-account-id")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal(_userId, accountEntity.PartitionKey);
            Assert.Equal("id-account-id", accountEntity.RowKey);
            Assert.Equal("account-id", accountEntity.Id);
            Assert.Equal("test-name", accountEntity.Name);
            Assert.Equal("test-hint", accountEntity.Hint);
            Assert.Equal("test-notes", accountEntity.Notes);
            Assert.True(accountEntity.IsPinned);
            Assert.False(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>(_userId, $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal(_userId, accountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", accountHintEntity.RowKey);
            Assert.Equal("account-id", accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.StartDate);
        }

        [Fact]
        public async Task Handle_ExistingDeletedAccount_ThrowsExceptuon()
        {
            var now = DateTime.UtcNow;
            _entityTables.Accounts.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new IndexEntity
                {
                    EntityType = "IndexEntity",
                    PartitionKey = _userId,
                    RowKey = "name-test-name",
                    IndexedEntityId = "account-id"
                }),
                TableOperation.Insert(new AccountEntity
                {
                    EntityType = "AccountEntity",
                    PartitionKey = _userId,
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
                    PartitionKey = _userId,
                    RowKey = $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}",
                    AccountId = "account-id",
                    Hint = "test-hint",
                    StartDate = now
                })
            });

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

            Assert.Equal(3, _entityTables.Accounts.ExecuteQuery(new TableQuery()).Count());
            var indexEntity = (IndexEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(_userId, "name-test-name")).Result;
            Assert.Equal("IndexEntity", indexEntity.EntityType);
            Assert.Equal(_userId, indexEntity.PartitionKey);
            Assert.Equal("name-test-name", indexEntity.RowKey);
            Assert.Equal("account-id", indexEntity.IndexedEntityId);

            var accountEntity = (AccountEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(_userId, "id-account-id")).Result;
            Assert.Equal("AccountEntity", accountEntity.EntityType);
            Assert.Equal(_userId, accountEntity.PartitionKey);
            Assert.Equal("id-account-id", accountEntity.RowKey);
            Assert.Equal("account-id", accountEntity.Id);
            Assert.Equal("test-name", accountEntity.Name);
            Assert.Equal("test-hint", accountEntity.Hint);
            Assert.Equal("test-notes", accountEntity.Notes);
            Assert.True(accountEntity.IsPinned);
            Assert.True(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>(_userId, $"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal(_userId, accountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-{now:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", accountHintEntity.RowKey);
            Assert.Equal("account-id", accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.StartDate);
        }
    }
}