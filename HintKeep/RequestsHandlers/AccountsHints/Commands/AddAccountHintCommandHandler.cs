using System;
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
    public class AddAccountHintCommandHandler : IRequestHandler<AddAccountHintCommand, string>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;

        public AddAccountHintCommandHandler(IEntityTables entityTables, Session session)
            => (_entityTables, _session) = (entityTables, session);

        public async Task<string> Handle(AddAccountHintCommand command, CancellationToken cancellationToken)
        {
            var account = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(_session.UserId.ToEncodedKeyProperty(), $"id-{command.AccountId}".ToEncodedKeyProperty(), new List<string> { nameof(AccountEntity.IsDeleted) }),
                cancellationToken
            )).Result ?? throw new NotFoundException();
            if (account.IsDeleted)
                throw new NotFoundException();

            var hintId = Guid.NewGuid().ToString("N");
            var accountHint = new AccountHintEntity
            {
                EntityType = "AccountHintEntity",
                PartitionKey = _session.UserId.ToEncodedKeyProperty(),
                RowKey = $"id-{command.AccountId}-hintId-{hintId}".ToEncodedKeyProperty(),
                AccountId = command.AccountId,
                HintId = hintId,
                Hint = command.Hint,
                DateAdded = command.DateAdded
            };
            var currentLatestHint = await _entityTables.GetLatestHint(_session.UserId, command.AccountId, cancellationToken);
            var latestHint = AccountHintEntitySortOrderComparer.Compare(accountHint, currentLatestHint) < 0 ? accountHint : currentLatestHint;

            await _entityTables.Accounts.ExecuteBatchAsync(
                new TableBatchOperation
                {
                    TableOperation.Insert(accountHint),
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
            return hintId;
        }
    }
}