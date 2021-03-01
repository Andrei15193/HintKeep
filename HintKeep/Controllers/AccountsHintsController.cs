using System;
using System.Threading.Tasks;
using HintKeep.Requests.AccountsHints.Commands;
using HintKeep.Requests.AccountsHints.Queries;
using HintKeep.ViewModels.AccountsHints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers
{
    [ApiController, Route("api/accounts/{accountId}/hints")]
    public class AccountsHintsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsHintsController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAsync(string accountId)
        {
            var accountHints = await _mediator.Send(new AccountHintsQuery
            {
                AccountId = accountId
            });
            return Ok(accountHints);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(string accountId, AccountHintCreation accountHintCreation)
        {
            var hintId = await _mediator.Send(new AddAccountHintCommand
            {
                AccountId = accountId,
                Hint = accountHintCreation.Hint,
                DateAdded = accountHintCreation.DateAdded
            });
            return Created(new Uri($"/api/accounts/{accountId}/hints/{hintId}", UriKind.Relative), null);
        }

        [HttpPut("{hintId}")]
        public async Task<IActionResult> PutAsync(string accountId, string hintId, AccountHintUpdate accountHintUpdate)
        {
            await _mediator.Send(new UpdateAccountHintCommand
            {
                AccountId = accountId,
                HintId = hintId,
                DateAdded = accountHintUpdate.DateAdded
            });
            return NoContent();
        }

        [HttpDelete("{hintId}")]
        public async Task<IActionResult> DeleteAsync(string accountId, string hintId)
        {
            await _mediator.Send(new DeleteAccountHintCommand
            {
                AccountId = accountId,
                HintId = hintId
            });
            return NoContent();
        }
    }
}