using System.Collections.Generic;
using HintKeep.ViewModels.Accounts;
using MediatR;

namespace HintKeep.Requests.Accounts.Queries
{
    public class GetAccountsQuery : IRequest<IReadOnlyList<AccountSummary>>
    {
    }
}