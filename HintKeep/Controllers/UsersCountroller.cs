using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.Requests.Users.Queries;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
            => _mediator = mediator;

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> Post(UserSignUp userSignUp)
        {
            await _mediator.Send(new UserSignUpCommand
            {
                Email = userSignUp.Email,
                Password = userSignUp.Password
            });
            return Created(new Uri("/users/confirmations", UriKind.Relative), null);
        }

        [AllowAnonymous, HttpPost("confirmations")]
        public async Task<IActionResult> PostConfirmation(UserConfirmation userConfirmation)
        {
            await _mediator.Send(new UserRegistrationConfirmationCommand
            {
                Email = userConfirmation.Email,
                ConfirmationToken = userConfirmation.ConfirmationToken
            });
            return Created(new Uri("/users", UriKind.Relative), string.Empty);
        }

        [AllowAnonymous, HttpPost("authentications")]
        public async Task<IActionResult> PostAuthentication(UserLogin userLogin)
        {
            var userInfo = await _mediator.Send(new UserAuthenticationQuery
            {
                Email = userLogin.Email,
                Password = userLogin.Password
            });
            return Created(new Uri("/users/authentications", UriKind.Relative), userInfo);
        }

        [HttpDelete("authentications")]
        public IActionResult DeleteAuthentication(bool? current)
        {
            if (current ?? Request.Query.ContainsKey(nameof(current)))
                return NoContent();
            else
                return BadRequest(string.Empty);
        }
    }
}