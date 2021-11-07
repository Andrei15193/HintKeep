using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public class UserRequestHintCommand : IRequest
    {
        public string Email { get; set; }
    }
}