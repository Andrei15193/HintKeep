using System;
using MediatR;

namespace HintKeep.Requests.AccountsHints.Commands
{
    public class AddAccountHintCommand : IRequest<string>
    {
        public string AccountId { get; set; }

        public string Hint { get; set; }

        public DateTime? DateAdded { get; set; }
    }
}