using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public class RegisterUserCommand : IRequest
    {
        public string Email { get; init; }

        public string Hint { get; init; }

        public string Password { get; init; }
    }
}