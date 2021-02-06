using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public class MoveAccountToBinCommand : IRequest
    {
        public string Id { get; set; }
    }
}