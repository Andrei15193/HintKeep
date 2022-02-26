using System.Collections.Generic;
using HintKeep.ViewModels.AccountsHints;
using MediatR;

namespace HintKeep.Requests.AccountsHints.Queries
{
    public record AccountHintsQuery(
        string AccountId
    ) : IRequest<IEnumerable<AccountHintDetails>>;
}