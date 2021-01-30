using System;
using System.Threading.Tasks;
using HintKeep.Requests.Accounts.Commands;
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
    }
}