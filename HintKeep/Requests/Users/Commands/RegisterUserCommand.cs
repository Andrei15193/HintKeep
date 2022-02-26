using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public record RegisterUserCommand(
        string Email,
        string Hint,
        string Password
    ) : IRequest;
}