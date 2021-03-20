using System.Threading.Tasks;
using HintKeep.Requests.Accounts.Commands;
using HintKeep.Requests.Accounts.Queries;
using HintKeep.ViewModels.Accounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers
{
    [ApiController, Route("api/deleted-accounts")]
    public class DeletedAccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DeletedAccountsController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAsync()
            => Ok(await _mediator.Send(new GetDeletedAccountsQuery()));

        [HttpPut("{accountId}")]
        public async Task<IActionResult> PutAsync(string accountId, DeletedAccountUpdate deletedAccountUpdate)
        {
            await _mediator.Send(new UpdateDeletedAccountCommand
            {
                Id = accountId,
                IsDeleted = deletedAccountUpdate.IsDeleted
            });

            return NoContent();
        }
    }
}