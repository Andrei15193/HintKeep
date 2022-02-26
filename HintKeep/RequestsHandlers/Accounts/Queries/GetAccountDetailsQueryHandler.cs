using System.Collections.Generic;
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
        private readonly Session _login;

        public GetAccountDetailsQueryHandler(IEntityTables entityTables, Session login)
            => (_entityTables, _login) = (entityTables, login);

        public async Task<AccountDetails> Handle(GetAccountDetailsQuery query, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(
                    _login.UserId.ToEncodedKeyProperty(),
                    $"id-{query.Id}".ToEncodedKeyProperty(),
                    new List<string>
                    {
                        nameof(AccountEntity.Id),
                        nameof(AccountEntity.Name),
                        nameof(AccountEntity.Hint),
                        nameof(AccountEntity.Notes),
                        nameof(AccountEntity.IsPinned),
                        nameof(AccountEntity.IsDeleted)
                    }
                ),
                cancellationToken
            )).Result;
            if (accountEntity is null || accountEntity.IsDeleted)
                throw new NotFoundException();

            return new AccountDetails(accountEntity.Id, accountEntity.Name, accountEntity.Hint, accountEntity.Notes, accountEntity.IsPinned);
        }
    }
}