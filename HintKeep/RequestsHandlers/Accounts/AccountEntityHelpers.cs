using System;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Accounts
{
    public static class AccountEntityHelpers
    {
        public static Task EnsureAccountLatestHintAsync(this IEntityTables entityTables, AccountEntity accountEntity)
            => entityTables.EnsureAccountLatestHintAsync(accountEntity, CancellationToken.None);

        public static async Task EnsureAccountLatestHintAsync(this IEntityTables entityTables, AccountEntity accountEntity, CancellationToken cancellationToken)
        {
            var latestHint = accountEntity.Hint;
            if (latestHint is null)
            {
                var continuationToken = default(TableContinuationToken);
                var latestHintDateAdded = default(DateTime?);
                var accoutHintQuery = new TableQuery<AccountHintEntity>()
                    .Where(TableQuery.GenerateFilterCondition(nameof(AccountHintEntity.PartitionKey), QueryComparisons.Equal, $"accountId-{accountEntity.Id}"))
                    .Select(new[] { nameof(AccountHintEntity.Hint), nameof(AccountHintEntity.DateAdded) });
                do
                {
                    var result = await entityTables.AccountHints.ExecuteQuerySegmentedAsync(accoutHintQuery, continuationToken, cancellationToken);
                    continuationToken = result.ContinuationToken;
                    foreach (var accountHintEntity in result)
                        if (latestHint is null || accountHintEntity.DateAdded > latestHintDateAdded)
                        {
                            latestHintDateAdded = accountHintEntity.DateAdded;
                            latestHint = accountHintEntity.Hint;
                        }
                } while (continuationToken is not null);

                if (latestHint is not null)
                {
                    if ((DateTimeOffset.UtcNow - accountEntity.Timestamp.ToUniversalTime()).TotalHours >= 1)
                    {
                        var resultEntity = await entityTables.Accounts.ExecuteAsync(
                            TableOperation.Merge(new DynamicTableEntity
                            {
                                PartitionKey = accountEntity.PartitionKey,
                                RowKey = accountEntity.RowKey,
                                ETag = accountEntity.ETag,
                                Properties =
                                {
                                { nameof(AccountEntity.Hint), EntityProperty.GeneratePropertyForString(latestHint) }
                                }
                            }),
                            cancellationToken
                        );
                        accountEntity.ETag = resultEntity.Etag;
                    }
                    accountEntity.Hint = latestHint;
                }
            }
        }
    }
}