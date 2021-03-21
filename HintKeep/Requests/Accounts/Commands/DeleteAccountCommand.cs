using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public class DeleteAccountCommand : IRequest
    {
        public string Id { get; set; }
    }
}