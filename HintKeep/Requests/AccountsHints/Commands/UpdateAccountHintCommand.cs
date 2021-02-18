using System;
using MediatR;

namespace HintKeep.Requests.AccountsHints.Commands
{
    public class UpdateAccountHintCommand : IRequest
    {
        public string AccountId { get; set; }

        public string HintId { get; set; }

        public DateTime? DateAdded { get; set; }
    }
}