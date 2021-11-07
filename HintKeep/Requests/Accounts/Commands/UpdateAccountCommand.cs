using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public class UpdateAccountCommand : IRequest
    {
        public string Id { get; init; }

        public string Name { get; init; }

        public string Hint { get; init; }

        public string Notes { get; init; }

        public bool IsPinned { get; init; }
    }
}