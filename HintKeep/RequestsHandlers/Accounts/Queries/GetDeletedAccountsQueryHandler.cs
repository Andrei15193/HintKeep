using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Requests.Accounts.Queries;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.ViewModels.Accounts;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Accounts.Queries
{
    public class GetDeletedAccountsQueryHandler : IRequestHandler<GetDeletedAccountsQuery, IReadOnlyList<AccountSummary>>
    {
        private readonly IEntityTables _entityTables;
        private readonly Session _login;

        public GetDeletedAccountsQueryHandler(IEntityTables entityTables, Session login)
            => (_entityTables, _login) = (entityTables, login);

        public async Task<IReadOnlyList<AccountSummary>> Handle(GetDeletedAccountsQuery query, CancellationToken cancellationToken)
        {
            var tableQuery = new TableQuery<AccountEntity>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, _login.UserId.ToEncodedKeyProperty()),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "AccountEntity")
                        ),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForBool(nameof(AccountEntity.IsDeleted), QueryComparisons.Equal, true)
                    )
                )
                .Select(
                    new List<string>
                    {
                        nameof(AccountEntity.Id),
                        nameof(AccountEntity.Name),
                        nameof(AccountEntity.Hint),
                        nameof(AccountEntity.IsPinned)
                    }
                );
            var accounts = new List<AccountSummary>();
            var continuationToken = default(TableContinuationToken);
            do
            {
                var result = await _entityTables.Accounts.ExecuteQuerySegmentedAsync(tableQuery, continuationToken, cancellationToken);
                continuationToken = result.ContinuationToken;
                accounts.AddRange(
                    from accountEntity in result
                    select new AccountSummary
                    {
                        Id = accountEntity.Id,
                        Name = accountEntity.Name,
                        Hint = accountEntity.Hint,
                        IsPinned = accountEntity.IsPinned
                    }
                );
            } while (continuationToken is object);
            accounts.Sort(new AccountSummarySortOrderComparer());

            return accounts;
        }
    }
}