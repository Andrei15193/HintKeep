using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.AccountsHints.Queries;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.ViewModels.AccountsHints;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.AccountsHints.Queries
{
    public class AccountHintsQueryHandler : IRequestHandler<AccountHintsQuery, IEnumerable<AccountHintDetails>>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _session;

        public AccountHintsQueryHandler(IEntityTables entityTables, Session session)
            => (_entityTables, _session) = (entityTables, session);

        public async Task<IEnumerable<AccountHintDetails>> Handle(AccountHintsQuery query, CancellationToken cancellationToken)
        {
            var account = (AccountEntity)(await _entityTables.Accounts.ExecuteAsync(
                TableOperation.Retrieve<AccountEntity>(_session.UserId.ToEncodedKeyProperty(), $"id-{query.AccountId}".ToEncodedKeyProperty(), new List<string> { nameof(AccountEntity.IsDeleted) }),
                cancellationToken
            )).Result ?? throw new NotFoundException();
            if (account.IsDeleted)
                throw new NotFoundException();

            var accountHints = new List<AccountHintEntity>();
            var tableQuery = new TableQuery<AccountHintEntity>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, _session.UserId.ToEncodedKeyProperty()),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.GreaterThan, $"id-{query.AccountId}-hintId-".ToEncodedKeyProperty()),
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")
                            ),
                            TableOperators.And,
                            TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.LessThan, $"id-{query.AccountId}-hintId-z".ToEncodedKeyProperty()),
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountHintEntity")
                            )
                        )
                    )
                )
                .Select(new[] { nameof(AccountHintEntity.Hint), nameof(AccountHintEntity.HintId), nameof(AccountHintEntity.DateAdded) });
            var continuationToken = default(TableContinuationToken);
            do
            {
                var accountHintsSegment = await _entityTables.Accounts.ExecuteQuerySegmentedAsync(tableQuery, continuationToken, cancellationToken);
                continuationToken = accountHintsSegment.ContinuationToken;
                accountHints.AddRange(accountHintsSegment);
            } while (continuationToken is object);
            accountHints.Sort(new AccountHintEntitySortOrderComparer());

            return accountHints
                .Select(accountHint => new AccountHintDetails
                {
                    Id = accountHint.HintId,
                    Hint = accountHint.Hint,
                    DateAdded = accountHint.DateAdded
                })
                .ToArray();
        }
    }
}