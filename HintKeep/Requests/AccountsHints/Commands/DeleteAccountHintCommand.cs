using MediatR;

namespace HintKeep.Requests.AccountsHints.Commands
{
    public class DeleteAccountHintCommand : IRequest
    {
        public string AccountId { get; set; }

        public string HintId { get; set; }
    }
}