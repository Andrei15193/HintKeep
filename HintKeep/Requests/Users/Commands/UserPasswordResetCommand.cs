using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public class UserPasswordResetCommand : IRequest
    {
        public string Email { get; set; }

        public string Token { get; set; }

        public string Password { get; set; }
    }
}