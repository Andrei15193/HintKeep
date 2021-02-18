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
        public static async Task<AccountHintEntity> GetLatestHint(this IEntityTables entityTables, AccountHintEntity updatedAccountHint, IEnumerable<string> selectedColumns, CancellationToken cancellationToken)
        {
            var latestAccountHint = updatedAccountHint;
            var query = new TableQuery<AccountHintEntity>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, updatedAccountHint.PartitionKey),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.GreaterThan, $"id-{updatedAccountHint.AccountId}-hintId-".ToEncodedKeyProperty()),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")
                        )
                    )
                )
                .Select(selectedColumns.Concat(new[] { nameof(AccountHintEntity.HintId), nameof(AccountHintEntity.DateAdded) }).ToArray());
            var continuationToken = default(TableContinuationToken);
            do
            {
                var accountHintsSegment = await entityTables.Accounts.ExecuteQuerySegmentedAsync(query, continuationToken, cancellationToken);
                continuationToken = accountHintsSegment.ContinuationToken;
                foreach (var accountHint in accountHintsSegment.Select(accountHint => accountHint.HintId == updatedAccountHint.HintId ? updatedAccountHint : accountHint))
                    if (accountHint.DateAdded is object && (latestAccountHint.DateAdded is null || latestAccountHint.DateAdded < accountHint.DateAdded))
                        latestAccountHint = accountHint;
            } while (continuationToken is object);

            return latestAccountHint;
        }
    }
}