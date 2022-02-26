using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public record UserRequestHintCommand(
        string Email
    ) : IRequest;
}