using HintKeep.ViewModels.Accounts;
using MediatR;

namespace HintKeep.Requests.Accounts.Queries
{
    public record GetDeletedAccountDetailsQuery(
        string Id
    ) : IRequest<AccountDetails>;
}