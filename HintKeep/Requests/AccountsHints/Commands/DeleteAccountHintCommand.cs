using MediatR;

namespace HintKeep.Requests.AccountsHints.Commands
{
    public class DeleteAccountHintCommand : IRequest
    {
        public string AccountId { get; init; }

        public string HintId { get; init; }
    }
}