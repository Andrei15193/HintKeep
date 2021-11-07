using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public class ConfirmUserCommand : IRequest
    {
        public string Token { get; init; }
    }
}