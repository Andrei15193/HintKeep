using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public class UpdateDeletedAccountCommand : IRequest
    {
        public string Id { get; init; }

        public bool IsDeleted { get; init; }
    }
}