using System;
using MediatR;

namespace HintKeep.Requests.AccountsHints.Commands
{
    public record UpdateAccountHintCommand(
        string AccountId,
        string HintId,
        DateTime? DateAdded
    ) : IRequest;
}