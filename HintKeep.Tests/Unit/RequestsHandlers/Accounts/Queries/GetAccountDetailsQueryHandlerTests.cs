using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
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
            var account = new Account
            {
                UserId = _userId,
                IsDeleted = true
            };
            _entityTables.AddAccounts(account);

            var accountDetails = await _getAccountsQueryHandler.Handle(new GetAccountDetailsQuery { Id = account.Id }, CancellationToken.None);

            Assert.Equal(
                new
                {
                    account.Id,
                    account.Name,
                    account.Hints.Single().Hint,
                    account.Notes,
                    account.IsPinned,
                    account.IsDeleted
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