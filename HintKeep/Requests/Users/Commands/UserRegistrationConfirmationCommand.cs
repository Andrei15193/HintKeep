using MediatR;

namespace HintKeep.Requests.Users.Commands
{
    public class UserRegistrationConfirmationCommand : IRequest
    {
        public string Email { get; set; }

        public string ConfirmationToken { get; set; }
    }
}