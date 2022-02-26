using System;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Users
{
    [ApiController, Route("api/users/confirmations")]
    public class UsersConfirmationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersConfirmationsController(IMediator mediator)
            => _mediator = mediator;

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync(UserRegistrationConfirmation userRegistrationConfirmation)
        {
            await _mediator.Send(new ConfirmUserCommand(userRegistrationConfirmation.Token));

            return Created(new Uri("/api/users/sessions", UriKind.Relative), null);
        }
    }
}