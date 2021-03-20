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
    public class GetDeletedAccountDetailsQueryHandler : IRequestHandler<GetDeletedAccountDetailsQuery, AccountDetails>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _login;

        public GetDeletedAccountDetailsQueryHandler(IEntityTables entityTables, Session login)
            => (_entityTables, _login) = (entityTables, login);

        public async Task<AccountDetails> Handle(GetDeletedAccountDetailsQuery query, CancellationToken cancellationToken)
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
            if (accountEntity is null || !accountEntity.IsDeleted)
                throw new NotFoundException();

            return new AccountDetails
            {
                Id = accountEntity.Id,
                Name = accountEntity.Name,
                Hint = accountEntity.Hint,
                Notes = accountEntity.Notes,
                IsPinned = accountEntity.IsPinned
            };
        }
    }
}