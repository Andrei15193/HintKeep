using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.AccountsHints
{
    public static class Extensions
    {
        public static Task<AccountHintEntity> GetLatestHint(this IEntityTables entityTables, string userId, string accountId, CancellationToken cancellationToken)
            => entityTables.GetLatestHint(userId, accountId, Array.Empty<string>(), null, cancellationToken);

        public static Task<AccountHintEntity> GetLatestHint(this IEntityTables entityTables, string userId, string accountId, IEnumerable<string> selectedColumns, CancellationToken cancellationToken)
            => entityTables.GetLatestHint(userId, accountId, selectedColumns, null, cancellationToken);

        public static async Task<AccountHintEntity> GetLatestHint(this IEntityTables entityTables, string userId, string accountId, IEnumerable<string> selectedColumns, Func<AccountHintEntity, AccountHintEntity> selector, CancellationToken cancellationToken)
        {
            var latestAccountHint = default(AccountHintEntity);
            var query = new TableQuery<AccountHintEntity>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, userId.ToEncodedKeyProperty()),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.GreaterThan, $"id-{accountId}-hintId-".ToEncodedKeyProperty()),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")
                        )
                    )
                )
                .Select(new[] { nameof(AccountHintEntity.Hint), nameof(AccountHintEntity.DateAdded) }.Concat(selectedColumns ?? Array.Empty<string>()).ToArray());
            var continuationToken = default(TableContinuationToken);
            do
            {
                var accountHintsSegment = await entityTables.Accounts.ExecuteQuerySegmentedAsync(query, continuationToken, cancellationToken);
                continuationToken = accountHintsSegment.ContinuationToken;
                foreach (var accountHint in (selector is null ? accountHintsSegment : accountHintsSegment.Select(selector)))
                    if (AccountHintEntitySortOrderComparer.Compare(accountHint, latestAccountHint) < 0)
                        latestAccountHint = accountHint;
            } while (continuationToken is object);

            return latestAccountHint;
        }
    }
}