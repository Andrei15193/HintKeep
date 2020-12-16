using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public class UserSignUpCommand : IRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}