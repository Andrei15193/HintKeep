using System.Collections.Generic;
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
    public class UpdateAccountHintCommandHandler : AsyncRequestHandler<UpdateAccountHintCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;

        public UpdateAccountHintCommandHandler(IEntityTables entityTables, Session session)
            => (_entityTables, _session) = (entityTables, session);

        protected override async Task Handle(UpdateAccountHintCommand command, CancellationToken cancellationToken)
        {
            var account = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(_session.UserId.ToEncodedKeyProperty(), $"id-{command.AccountId}".ToEncodedKeyProperty(), new List<string> { nameof(AccountEntity.IsDeleted) }),
                cancellationToken
            )).Result ?? throw new NotFoundException();
            if (account.IsDeleted)
                throw new NotFoundException();

            var updatedAccountHint = (AccountHintEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountHintEntity>(_session.UserId.ToEncodedKeyProperty(), $"id-{command.AccountId}-hintId-{command.HintId}".ToEncodedKeyProperty()),
                cancellationToken
            )).Result ?? throw new NotFoundException();

            updatedAccountHint.DateAdded = command.DateAdded;
            var latestHint = await _entityTables.GetLatestHint(
                _session.UserId,
                command.AccountId,
                new[] { nameof(AccountHintEntity.HintId) },
                accountHint => accountHint.HintId == command.HintId ? updatedAccountHint : accountHint,
                cancellationToken
            );

            await _entityTables.Accounts.ExecuteBatchAsync(
                new TableBatchOperation
                {
                    TableOperation.Replace(updatedAccountHint),
                    TableOperation.Merge(new DynamicTableEntity
                    {
                        PartitionKey = account.PartitionKey,
                        RowKey = account.RowKey,
                        ETag = account.ETag,
                        Properties =
                        {
                            { nameof(AccountEntity.Hint), EntityProperty.GeneratePropertyForString(latestHint.Hint) }
                        }
                    })
                },
                cancellationToken
            );
        }
    }
}