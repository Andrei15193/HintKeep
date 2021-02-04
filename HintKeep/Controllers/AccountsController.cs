using System;
using System.Threading.Tasks;
using HintKeep.Requests.Accounts.Commands;
using HintKeep.Requests.Accounts.Queries;
using HintKeep.ViewModels.Accounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAsync()
            => Ok(await _mediator.Send(new GetAccountsQuery()));

        [HttpPost]
        public async Task<IActionResult> PostAsync(AccountCreation accountCreation)
        {
            var accountId = await _mediator.Send(new CreateAccountCommand
            {
                Name = accountCreation.Name,
                Hint = accountCreation.Hint,
                IsPinned = accountCreation.IsPinned
            });

            return Created(new Uri($"/accounts/{accountId}", UriKind.Relative), null);
        }

        [HttpPut("/accounts/{accountId}")]
        public async Task<IActionResult> PutAsync([FromRoute] string accountId, [FromBody] AccountUpdate accountUpdate)
        {
            await _mediator.Send(new UpdateAccountCommand
            {
                Id = accountId,
                Name = accountUpdate.Name,
                Hint = accountUpdate.Hint,
                IsPinned = accountUpdate.IsPinned
            });

            return NoContent();
        }
    }
}