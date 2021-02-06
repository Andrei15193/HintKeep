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
    public class MoveAccountToBinCommandHandlerTests
    {
        private readonly string _userId;
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<MoveAccountToBinCommand> _moveAccountToBinCommandHandler;

        public MoveAccountToBinCommandHandlerTests()
        {
            _userId = Guid.NewGuid().ToString("N");
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _moveAccountToBinCommandHandler = new MoveAccountToBinCommandHandler(_entityTables, new LoginInfo(_userId));
        }

        [Fact]
        public async Task Handle_WhenAccountDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _moveAccountToBinCommandHandler.Handle(new MoveAccountToBinCommand { Id = "account-id" }, CancellationToken.None));
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenAccountExist_MarksItAsDeleted()
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
                    IsPinned = true,
                    IsDeleted = false
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = _userId,
                    RowKey = "id-account-id-hintDate-1",
                    AccountId = "account-id",
                    Hint ="test-hint",
                    StartDate = now
                })
            });

            await _moveAccountToBinCommandHandler.Handle(new MoveAccountToBinCommand { Id = "account-id" }, CancellationToken.None);

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
            Assert.True(accountEntity.IsPinned);
            Assert.True(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>(_userId, "id-account-id-hintDate-1")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal(_userId, accountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-1", accountHintEntity.RowKey);
            Assert.Equal("account-id", accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.StartDate);
        }

        [Fact]
        public async Task Handle_WhenAccountIsDeleted_ThrowsException()
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
                    IsPinned = true,
                    IsDeleted = true
                }),
                TableOperation.Insert(new AccountHintEntity
                {
                    EntityType = "AccountHintEntity",
                    PartitionKey = _userId,
                    RowKey = "id-account-id-hintDate-1",
                    AccountId = "account-id",
                    Hint ="test-hint",
                    StartDate = now
                })
            });

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _moveAccountToBinCommandHandler.Handle(new MoveAccountToBinCommand { Id = "account-id" }, CancellationToken.None));
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
            Assert.True(accountEntity.IsPinned);
            Assert.True(accountEntity.IsDeleted);

            var accountHintEntity = (AccountHintEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountHintEntity>(_userId, "id-account-id-hintDate-1")).Result;
            Assert.Equal("AccountHintEntity", accountHintEntity.EntityType);
            Assert.Equal(_userId, accountHintEntity.PartitionKey);
            Assert.Equal($"id-account-id-hintDate-1", accountHintEntity.RowKey);
            Assert.Equal("account-id", accountHintEntity.AccountId);
            Assert.Equal("test-hint", accountHintEntity.Hint);
            Assert.Equal(now, accountHintEntity.StartDate);
        }
    }
}