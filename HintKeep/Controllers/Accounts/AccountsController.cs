using System;
using System.Threading.Tasks;
using HintKeep.Requests.Accounts.Commands;
using HintKeep.Requests.Accounts.Queries;
using HintKeep.ViewModels.Accounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Accounts
{
    [ApiController, Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAsync()
            => Ok(await _mediator.Send(new GetAccountsQuery()));

        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetAsync(string accountId)
            => Ok(await _mediator.Send(new GetAccountDetailsQuery(accountId)));

        [HttpPost]
        public async Task<IActionResult> PostAsync(AccountCreation accountCreation)
        {
            var accountId = await _mediator.Send(new AddAccountCommand(
                accountCreation.Name,
                accountCreation.Hint,
                accountCreation.Notes,
                accountCreation.IsPinned
            ));

            return Created(new Uri($"/api/accounts/{accountId}", UriKind.Relative), null);
        }

        [HttpPut("{accountId}")]
        public async Task<IActionResult> PutAsync(string accountId, AccountUpdate accountUpdate)
        {
            await _mediator.Send(new UpdateAccountCommand(
                accountId,
                accountUpdate.Name,
                accountUpdate.Hint,
                accountUpdate.Notes,
                accountUpdate.IsPinned
            ));

            return NoContent();
        }

        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAsync(string accountId)
        {
            await _mediator.Send(new MoveAccountToBinCommand(accountId));

            return NoContent();
        }
    }
}