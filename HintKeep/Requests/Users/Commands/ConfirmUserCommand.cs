using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public record ConfirmUserCommand(
        string Token
    ) : IRequest;
}