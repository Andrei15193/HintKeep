using HintKeep.ViewModels.Users;
using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public class CreateUserSessionCommand : IRequest<UserSession>
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}