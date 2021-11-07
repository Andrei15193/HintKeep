using System.Threading.Tasks;
using HintKeep.Requests.Accounts.Commands;
using HintKeep.Requests.Accounts.Queries;
using HintKeep.ViewModels.Accounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Accounts
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

        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetAsync(string accountId)
            => Ok(await _mediator.Send(new GetDeletedAccountDetailsQuery { Id = accountId }));

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

        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAsync(string accountId)
        {
            await _mediator.Send(new DeleteAccountCommand
            {
                Id = accountId
            });

            return NoContent();
        }
    }
}