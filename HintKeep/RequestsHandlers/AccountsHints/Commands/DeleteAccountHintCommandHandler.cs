using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.AccountsHints.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.AccountsHints.Commands
{
    public class DeleteAccountHintCommandHandler : AsyncRequestHandler<DeleteAccountHintCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;

        public DeleteAccountHintCommandHandler(IEntityTables entityTables, Session session)
            => (_entityTables, _session) = (entityTables, session);

        protected override async Task Handle(DeleteAccountHintCommand command, CancellationToken cancellationToken)
        {
            var accountEntity = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(_session.UserId.ToEncodedKeyProperty(), $"accountId-{command.AccountId}".ToEncodedKeyProperty()),
                cancellationToken
            )).Result;
            if (accountEntity is null || accountEntity.IsDeleted)
                throw new NotFoundException();

            try
            {
                accountEntity.Hint = null;
                await _entityTables.Accounts.ExecuteAsync(TableOperation.Replace(accountEntity), cancellationToken);

                await _entityTables.AccountHints.ExecuteAsync(
                    TableOperation.Delete(new TableEntity
                    {
                        PartitionKey = $"accountId-{command.AccountId}".ToEncodedKeyProperty(),
                        RowKey = $"hintId-{command.HintId}".ToEncodedKeyProperty(),
                        ETag = "*"
                    }),
                    cancellationToken
                );
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new NotFoundException(storageException);
            }
        }
    }
}