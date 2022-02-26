using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public record UpdateAccountCommand(
        string Id,
        string Name,
        string Hint,
        string Notes,
        bool IsPinned
    ) : IRequest;
}