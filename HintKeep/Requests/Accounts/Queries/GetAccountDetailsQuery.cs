using HintKeep.ViewModels.Accounts;
using MediatR;

namespace HintKeep.Requests.Accounts.Queries
{
    public class GetAccountDetailsQuery : IRequest<AccountDetails>
    {
        public string Id { get; init; }
    }
}