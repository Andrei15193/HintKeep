using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public class UserRequestPasswordResetCommand : IRequest
    {
        public string Email { get; set; }
    }
}