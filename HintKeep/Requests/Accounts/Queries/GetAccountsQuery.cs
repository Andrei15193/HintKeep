using System.Collections.Generic;
using HintKeep.ViewModels.Accounts;
using MediatR;

namespace HintKeep.Requests.Accounts.Queries
{
    public record GetAccountsQuery() : IRequest<IReadOnlyList<AccountSummary>>;
}