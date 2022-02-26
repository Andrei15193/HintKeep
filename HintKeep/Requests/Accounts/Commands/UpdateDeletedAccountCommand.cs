using MediatR;

namespace HintKeep.Requests.Accounts.Commands
{
    public record UpdateDeletedAccountCommand(
        string Id,
        bool IsDeleted
    ) : IRequest;
}