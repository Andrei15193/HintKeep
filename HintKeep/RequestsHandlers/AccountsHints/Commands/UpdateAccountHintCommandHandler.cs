using System.Collections.Generic;
using System.Linq;
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
                TableOperation.Retrieve<AccountEntity>(_session.UserId.ToEncodedKeyProperty(), $"id-{command.AccountId}".ToEncodedKeyProperty(), new List<string>()),
                cancellationToken
            )).Result ?? throw new NotFoundException();
            if (account.IsDeleted)
                throw new NotFoundException();

            var accountHint = (AccountHintEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountHintEntity>(_session.UserId.ToEncodedKeyProperty(), $"id-{command.AccountId}-hintId-{command.HintId}".ToEncodedKeyProperty()),
                cancellationToken
            )).Result ?? throw new NotFoundException();

            accountHint.DateAdded = command.DateAdded;
            var latestHint = await _GetLatestHint(accountHint, cancellationToken);

            await _entityTables.Accounts.ExecuteBatchAsync(
                new TableBatchOperation
                {
                    TableOperation.Replace(accountHint),
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

        private async Task<AccountHintEntity> _GetLatestHint(AccountHintEntity updatedAccountHint, CancellationToken cancellationToken)
        {
            var latestAccountHint = updatedAccountHint;
            var query = new TableQuery<AccountHintEntity>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, _session.UserId.ToEncodedKeyProperty()),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.GreaterThan, $"id-{updatedAccountHint.AccountId}-hintId-".ToEncodedKeyProperty()),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")
                        )
                    )
                )
                .Select(new List<string>
                {
                    nameof(AccountHintEntity.HintId),
                    nameof(AccountHintEntity.Hint),
                    nameof(AccountHintEntity.DateAdded)
                });
            var continuationToken = default(TableContinuationToken);
            do
            {
                var accountHintsSegment = await _entityTables.Accounts.ExecuteQuerySegmentedAsync(query, continuationToken, cancellationToken);
                continuationToken = accountHintsSegment.ContinuationToken;
                foreach (var accountHint in accountHintsSegment.Select(accountHint => accountHint.HintId == updatedAccountHint.HintId ? updatedAccountHint : accountHint))
                    if (accountHint.DateAdded is object && (latestAccountHint.DateAdded is null || latestAccountHint.DateAdded < accountHint.DateAdded))
                        latestAccountHint = accountHint;
            } while (continuationToken is object);

            return latestAccountHint;
        }
    }
}