using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public record AddAccountCommand(
        string Name,
        string Hint,
        string Notes,
        bool IsPinned
    ) : IRequest<string>;
}