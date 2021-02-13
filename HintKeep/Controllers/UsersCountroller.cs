using System;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers
{
    [ApiController, Route("users")]
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
            return Created(new Uri("/users/confirmations", UriKind.Relative), null);
        }

        [AllowAnonymous, HttpPost("confirmations")]
        public async Task<IActionResult> PostConfirmationAsync(UserConfirmation userConfirmation)
        {
            await _mediator.Send(new UserRegistrationConfirmationCommand
            {
                Email = userConfirmation.Email,
                ConfirmationToken = userConfirmation.ConfirmationToken
            });
            return Created(new Uri("/users", UriKind.Relative), string.Empty);
        }
    }
}