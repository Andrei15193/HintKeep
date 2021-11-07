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
            => Ok(await _mediator.Send(new GetAccountDetailsQuery { Id = accountId }));

        [HttpPost]
        public async Task<IActionResult> PostAsync(AccountCreation accountCreation)
        {
            var accountId = await _mediator.Send(new AddAccountCommand
            {
                Name = accountCreation.Name,
                Hint = accountCreation.Hint,
                Notes = accountCreation.Notes,
                IsPinned = accountCreation.IsPinned
            });

            return Created(new Uri($"/api/accounts/{accountId}", UriKind.Relative), null);
        }

        [HttpPut("{accountId}")]
        public async Task<IActionResult> PutAsync(string accountId, AccountUpdate accountUpdate)
        {
            await _mediator.Send(new UpdateAccountCommand
            {
                Id = accountId,
                Name = accountUpdate.Name,
                Hint = accountUpdate.Hint,
                Notes = accountUpdate.Notes,
                IsPinned = accountUpdate.IsPinned
            });

            return NoContent();
        }

        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAsync(string accountId)
        {
            await _mediator.Send(new MoveAccountToBinCommand
            {
                Id = accountId
            });

            return NoContent();
        }
    }
}