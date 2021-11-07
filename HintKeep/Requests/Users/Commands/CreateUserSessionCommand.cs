using MediatR;

namespace HintKeep.RequestsHandlers.Users.Commands
{
    public class CreateUserSessionCommand : IRequest<string>
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}