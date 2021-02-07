using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Accounts.Queries;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.ViewModels.Accounts;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Accounts.Queries
{
    public class GetAccountDetailsQueryHandler : IRequestHandler<GetAccountDetailsQuery, AccountDetails>
    {
        private readonly IEntityTables _entityTables;
        private readonly LoginInfo _login;

        public GetAccountDetailsQueryHandler(IEntityTables entityTables, LoginInfo login)
            => (_entityTables, _login) = (entityTables, login);

        public async Task<AccountDetails> Handle(GetAccountDetailsQuery query, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(TableOperation.Retrieve<AccountEntity>(_login.UserId, $"id-{query.Id}"), cancellationToken)).Result;
            if (accountEntity is null)
                throw new NotFoundException();

            return new AccountDetails
            {
                Id = accountEntity.Id,
                Name = accountEntity.Name,
                Hint = accountEntity.Hint,
                Notes = accountEntity.Notes,
                IsPinned = accountEntity.IsPinned,
                IsDeleted = accountEntity.IsDeleted
            };
        }
    }
}