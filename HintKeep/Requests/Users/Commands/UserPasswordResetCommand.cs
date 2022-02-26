using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public record UserPasswordResetCommand(
        string Email,
        string Token,
        string Password
    ) : IRequest;
}