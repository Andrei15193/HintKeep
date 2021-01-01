using HintKeep.ViewModels.Users;
using MediatR;

namespace HintKeep.Requests.Users.Queries
{
    public class UserAuthenticationQuery : IRequest<UserInfo>
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}