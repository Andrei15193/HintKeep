using MediatR;

namespace HintKeep.Commands.Users
{
    public class UserSignUpCommand : IRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}