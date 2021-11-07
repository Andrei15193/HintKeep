using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public class AddAccountCommand : IRequest<string>
    {
        public string Name { get; init; }

        public string Hint { get; init; }

        public string Notes { get; init; }

        public bool IsPinned { get; init; }
    }
}