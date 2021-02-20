using System;
using System.Threading.Tasks;
using HintKeep.Requests.AccountsHints.Commands;
using HintKeep.ViewModels.Accounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers
{
    [ApiController, Route("accounts/{accountId}/hints")]
    public class AccountsHintsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsHintsController(IMediator mediator)
            => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> PostAsync(string accountId, AccountHintCreation accountHintCreation)
        {
            var hintId = await _mediator.Send(new AddAccountHintCommand
            {
                AccountId = accountId,
                Hint = accountHintCreation.Hint,
                DateAdded = accountHintCreation.DateAdded
            });
            return Created(new Uri($"/accounts/{accountId}/hints/{hintId}", UriKind.Relative), null);
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
    }
}