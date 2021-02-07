using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public class CreateAccountCommand : IRequest<string>
    {
        public string Name { get; set; }

        public string Hint { get; set; }

        public string Notes { get; set; }

        public bool IsPinned { get; set; }
    }
}