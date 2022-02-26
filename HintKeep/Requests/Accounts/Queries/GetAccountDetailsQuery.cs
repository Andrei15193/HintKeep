using HintKeep.ViewModels.Accounts;
using MediatR;

namespace HintKeep.Requests.Accounts.Queries
{
    public record GetAccountDetailsQuery(
        string Id
    ) : IRequest<AccountDetails>;
}