using MediatR;

namespace HintKeep.Requests.AccountsHints.Commands
{
    public record DeleteAccountHintCommand(
        string AccountId,
        string HintId
    ) : IRequest;
}