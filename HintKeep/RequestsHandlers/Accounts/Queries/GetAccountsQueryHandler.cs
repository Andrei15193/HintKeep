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
    public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, IReadOnlyList<Account>>
    {
        private readonly IEntityTables _entityTables;
        private readonly LoginInfo _login;

        public GetAccountsQueryHandler(IEntityTables entityTables, LoginInfo login)
            => (_entityTables, _login) = (entityTables, login);

        public async Task<IReadOnlyList<Account>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        {
            var query = new TableQuery<AccountEntity>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, _login.UserId),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition(nameof(AccountEntity.Name), QueryComparisons.NotEqual, string.Empty)
                    )
                );
            var accounts = new List<Account>();
            var continuationToken = default(TableContinuationToken);
            do
            {
                var result = await _entityTables.Accounts.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = result.ContinuationToken;
                accounts.AddRange(
                    from accountEntity in result
                    select new Account
                    {
                        Id = accountEntity.Id,
                        Name = accountEntity.Name,
                        Hint = accountEntity.Hint,
                        IsPinned = accountEntity.IsPinned
                    }
                );
            } while (continuationToken is object);
            accounts.Sort(new AccountComparer());

            return accounts;
        }

        private sealed class AccountComparer : IComparer<Account>
        {
            public int Compare(Account left, Account right)
            {
                if (left.IsPinned == right.IsPinned)
                    return StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
                else if (left.IsPinned)
                    return -1;
                else
                    return 1;
            }
        }
    }
}