using System.Collections.Generic;
using HintKeep.ViewModels.AccountsHints;
using MediatR;

namespace HintKeep.Requests.AccountsHints.Queries
{
    public class AccountHintsQuery : IRequest<IEnumerable<AccountHintDetails>>
    {
        public string AccountId { get; set; }
    }
}