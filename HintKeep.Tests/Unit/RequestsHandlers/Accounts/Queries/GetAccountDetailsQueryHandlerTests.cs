using System;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
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
    public class GetAccountDetailsQueryHandlerTest
    {
        private readonly string _userId;
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<GetAccountDetailsQuery, AccountDetails> _getAccountsQueryHandler;

        public GetAccountDetailsQueryHandlerTest()
        {
            _userId = Guid.NewGuid().ToString("N");
            _entityTables = new InMemoryEntityTables();
            _entityTables.Accounts.Create();
            _getAccountsQueryHandler = new GetAccountDetailsQueryHandler(_entityTables, new LoginInfo(_userId));
        }

        [Fact]
        public async Task Handle_WhenAccountDoesNotExist_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _getAccountsQueryHandler.Handle(new GetAccountDetailsQuery { Id = "account-id" }, CancellationToken.None));
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task Handle_WhenAccountExists_ReturnsAccountDetails()
        {
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
                    RowKey = "id-account-id-hintDate-1",
                    AccountId = "account-id",
                    Hint = "test-hint",
                    StartDate = DateTime.UtcNow
                })
            });

            var accountDetails = await _getAccountsQueryHandler.Handle(new GetAccountDetailsQuery { Id = "account-id" }, CancellationToken.None);

            Assert.Equal(
                new
                {
                    Id = "account-id",
                    Name = "test-name",
                    Hint = "test-hint",
                    Notes = "test-notes",
                    IsPinned = true,
                    IsDeleted = true
                },
                new
                {
                    accountDetails.Id,
                    accountDetails.Name,
                    accountDetails.Hint,
                    accountDetails.Notes,
                    accountDetails.IsPinned,
                    accountDetails.IsDeleted
                }
            );
        }
    }
}