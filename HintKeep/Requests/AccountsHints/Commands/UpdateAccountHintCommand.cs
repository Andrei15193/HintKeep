using System;
using MediatR;

namespace HintKeep.Requests.AccountsHints.Commands
{
    public class UpdateAccountHintCommand : IRequest
    {
        public string AccountId { get; init; }

        public string HintId { get; init; }

        public DateTime? DateAdded { get; init; }
    }
}