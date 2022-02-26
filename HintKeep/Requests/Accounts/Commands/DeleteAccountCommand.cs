using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public record DeleteAccountCommand(
         string Id
    ) : IRequest;
}