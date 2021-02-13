using HintKeep.ViewModels.Users;
using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public class RefreshUserSessionCommand : IRequest<UserSession>
    {
    }
}