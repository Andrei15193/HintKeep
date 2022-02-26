using System;
using MediatR;

namespace HintKeep.Requests.AccountsHints.Commands
{
    public record AddAccountHintCommand(
        string AccountId,
        string Hint,
        DateTime? DateAdded
    ) : IRequest<string>;
}