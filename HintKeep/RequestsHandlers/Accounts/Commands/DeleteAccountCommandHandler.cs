using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Accounts.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Accounts.Commands
{
    public class DeleteAccountCommandHandler : AsyncRequestHandler<DeleteAccountCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _login;

        public DeleteAccountCommandHandler(IEntityTables entityTables, Session login)
            => (_entityTables, _login) = (entityTables, login);

        protected override async Task Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(
                    _login.UserId.ToEncodedKeyProperty(),
                    $"accountId-{command.Id}".ToEncodedKeyProperty(),
                    new List<string>
                    {
                        nameof(AccountEntity.Name),
                        nameof(AccountEntity.IsDeleted)
                    }
                ),
                cancellationToken
            )).Result;
            if (accountEntity is null || !accountEntity.IsDeleted)
                throw new NotFoundException();

            var indexEntity = (IndexEntity)(await _entityTables.Accounts.ExecuteAsync(TableOperation.Retrieve<IndexEntity>(_login.UserId.ToEncodedKeyProperty(), $"name-{accountEntity.Name.ToLowerInvariant()}".ToEncodedKeyProperty(), new List<string>()), cancellationToken)).Result;
            if (indexEntity is null)
                throw new NotFoundException();

            await _entityTables.Accounts.ExecuteBatchAsync(
                new TableBatchOperation
                {
                    TableOperation.Delete(accountEntity),
                    TableOperation.Delete(indexEntity)
                },
                cancellationToken
            );
        }
    }
}