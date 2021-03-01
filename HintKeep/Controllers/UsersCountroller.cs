using System;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers
{
    [ApiController, Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
            => _mediator = mediator;

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> PostAsync(UserSignUp userSignUp)
        {
            await _mediator.Send(new UserSignUpCommand
            {
                Email = userSignUp.Email,
                Password = userSignUp.Password
            });
            return Created(new Uri("/api/users/confirmations", UriKind.Relative), null);
        }

        [AllowAnonymous, HttpPost("confirmations")]
        public async Task<IActionResult> PostConfirmationAsync(UserConfirmation userConfirmation)
        {
            await _mediator.Send(new UserRegistrationConfirmationCommand
            {
                Email = userConfirmation.Email,
                ConfirmationToken = userConfirmation.ConfirmationToken
            });
            return Created(new Uri("/api/users", UriKind.Relative), string.Empty);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(bool? current)
        {
            if (current ?? Request.Query.ContainsKey(nameof(current)))
            {
                await _mediator.Send(new DeleteCurrentUserCommand());
                return NoContent();
            }
            else
                return BadRequest(string.Empty);
        }
    }
}