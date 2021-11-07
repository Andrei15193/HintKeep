using System;
using MediatR;

namespace HintKeep.Requests.AccountsHints.Commands
{
    public class AddAccountHintCommand : IRequest<string>
    {
        public string AccountId { get; init; }

        public string Hint { get; init; }

        public DateTime? DateAdded { get; init; }
    }
}