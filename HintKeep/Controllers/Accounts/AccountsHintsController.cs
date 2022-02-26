using System;
using System.Threading.Tasks;
using HintKeep.Requests.AccountsHints.Commands;
using HintKeep.Requests.AccountsHints.Queries;
using HintKeep.ViewModels.AccountsHints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Accounts
{
    [ApiController, Route("api/accounts/{accountId}/hints")]
    public class AccountsHintsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsHintsController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAsync(string accountId)
            => Ok(await _mediator.Send(new AccountHintsQuery(accountId)));

        [HttpPost]
        public async Task<IActionResult> PostAsync(string accountId, AccountHintCreation accountHintCreation)
        {
            var hintId = await _mediator.Send(new AddAccountHintCommand(
                accountId,
                accountHintCreation.Hint,
                accountHintCreation.DateAdded
            ));

            return Created(new Uri($"/api/accounts/{accountId}/hints/{hintId}", UriKind.Relative), null);
        }

        [HttpPut("{hintId}")]
        public async Task<IActionResult> PutAsync(string accountId, string hintId, AccountHintUpdate accountHintUpdate)
        {
            await _mediator.Send(new UpdateAccountHintCommand(
                accountId,
                hintId,
                accountHintUpdate.DateAdded
            ));

            return NoContent();
        }

        [HttpDelete("{hintId}")]
        public async Task<IActionResult> DeleteAsync(string accountId, string hintId)
        {
            await _mediator.Send(new DeleteAccountHintCommand(
                accountId,
                hintId
            ));

            return NoContent();
        }
    }
}