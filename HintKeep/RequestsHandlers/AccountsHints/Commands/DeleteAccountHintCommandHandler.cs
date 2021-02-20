using System.Collections.Generic;
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
            var account = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(_session.UserId.ToEncodedKeyProperty(), $"id-{command.AccountId}".ToEncodedKeyProperty(), new List<string> { nameof(AccountEntity.IsDeleted) }),
                cancellationToken
            )).Result ?? throw new NotFoundException();
            if (account.IsDeleted)
                throw new NotFoundException();

            var latestHint = await _entityTables.GetLatestHint(
                _session.UserId,
                command.AccountId,
                new[] { nameof(AccountHintEntity.HintId) },
                accountHint => accountHint.HintId == command.HintId ? null : accountHint,
                cancellationToken
            );

            try
            {
                await _entityTables.Accounts.ExecuteBatchAsync(
                    new TableBatchOperation
                    {
                        TableOperation.Delete(new TableEntity
                        {
                            PartitionKey = _session.UserId.ToEncodedKeyProperty(),
                            RowKey = $"id-{command.AccountId}-hintId-{command.HintId}".ToEncodedKeyProperty(),
                            ETag = "*"
                        }),
                        TableOperation.Merge(new DynamicTableEntity
                        {
                            PartitionKey = account.PartitionKey,
                            RowKey = account.RowKey,
                            ETag = account.ETag,
                            Properties =
                            {
                                { nameof(AccountEntity.Hint), EntityProperty.GeneratePropertyForString(latestHint?.Hint ?? string.Empty) }
                            }
                        })
                    },
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