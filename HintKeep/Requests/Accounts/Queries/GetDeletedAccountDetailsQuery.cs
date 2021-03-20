using HintKeep.ViewModels.Accounts;
using MediatR;

namespace HintKeep.Requests.Accounts.Queries
{
    public class GetDeletedAccountDetailsQuery : IRequest<AccountDetails>
    {
        public string Id { get; set; }
    }
}