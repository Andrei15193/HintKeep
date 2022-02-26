using MediatR;

namespace HintKeep.RequestsHandlers.Users.Commands
{
    public record CreateUserSessionCommand(
        string Email,
        string Password
    ) : IRequest<string>;
}