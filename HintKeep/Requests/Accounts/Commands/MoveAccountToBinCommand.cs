using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public record MoveAccountToBinCommand(
        string Id
    ) : IRequest;
}