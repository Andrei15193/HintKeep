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
                    IsPinned = true
                },
                CancellationToken.None
            );

            Assert.Equal(3, _entityTables.Accounts.ExecuteQuery(new TableQuery<AccountEntity>()).Count());

            var accountEntity = (AccountEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<AccountEntity>(_userId, $"id-{accountId}")).Result;
            Assert.Equal(_userId, accountEntity.PartitionKey);
            Assert.Equal($"id-{accountId}", accountEntity.RowKey);
            Assert.Equal("Test-Account", accountEntity.Name);
            Assert.Equal("Test-Hint", accountEntity.Hint);
            Assert.True(accountEntity.IsPinned);

            var indexedEntity = (IndexEntity)_entityTables.Accounts.Execute(TableOperation.Retrieve<IndexEntity>(_userId, "name-test-account")).Result;
            Assert.Equal(_userId, indexedEntity.PartitionKey);
            Assert.Equal("name-test-account", indexedEntity.RowKey);
            Assert.Equal(accountId, indexedEntity.IndexedEntityId);

            var accountHintEntity = Assert.Single(_entityTables.Accounts.ExecuteQuery(
                new TableQuery<AccountHintEntity>()
                    .Where(TableQuery.GenerateFilterCondition(nameof(AccountHintEntity.AccountId), QueryComparisons.NotEqual, string.Empty))
            ));
            Assert.Equal(_userId, accountHintEntity.PartitionKey);
            Assert.Equal($"id-{accountId}-hintDate-{accountHintEntity.StartDate:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}", accountHintEntity.RowKey);
            Assert.NotNull(accountHintEntity.StartDate);
            Assert.Equal(accountEntity.Id, accountHintEntity.AccountId);
            Assert.Equal("Test-Hint", accountHintEntity.Hint);
        }

        [Fact]
        public async Task Handle_ExistingAccount_ThrowsExceptuon()
        {
            _entityTables.Accounts.Execute(TableOperation.Insert(new IndexEntity
            {
                PartitionKey = _userId,
                RowKey = "name-test-account"
            }));

            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _createAccountCommandHandler.Handle(
                    new CreateAccountCommand
                    {
                        Name = "Test-Account",
                        Hint = "Test-Hint-2"
                    },
                    CancellationToken.None
                )
            );
            Assert.Empty(exception.Message);
        }
    }
}