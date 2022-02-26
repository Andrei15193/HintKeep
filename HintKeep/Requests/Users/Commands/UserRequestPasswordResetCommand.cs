using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public record UserRequestPasswordResetCommand(
        string Email
    ) : IRequest;
}