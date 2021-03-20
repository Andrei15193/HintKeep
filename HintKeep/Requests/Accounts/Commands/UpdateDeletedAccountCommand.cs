using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public class UpdateDeletedAccountCommand : IRequest
    {
        public string Id { get; set; }

        public bool IsDeleted { get; set; }
    }
}