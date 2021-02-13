using System;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers
{
    [ApiController, Route("users/sessions")]
    public class UsersSessionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersSessionsController(IMediator mediator)
            => _mediator = mediator;

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> PostSessionAsync(UserLogin userLogin)
        {
            var userSession = await _mediator.Send(new CreateUserSessionCommand
            {
                Email = userLogin.Email,
                Password = userLogin.Password
            });
            return Created(new Uri("/users/sessions", UriKind.Relative), userSession);
        }

        [HttpPut]
        public async Task<IActionResult> PutSessionAsync(bool? current)
        {
            if (current ?? Request.Query.ContainsKey(nameof(current)))
            {
                var userSession = await _mediator.Send(new RefreshUserSessionCommand());
                return Ok(userSession);
            }
            else
                return BadRequest(string.Empty);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSessionAsync(bool? current)
        {
            if (current ?? Request.Query.ContainsKey(nameof(current)))
            {
                await _mediator.Send(new DeleteCurrentUserSessionCommand());
                return NoContent();
            }
            else
                return BadRequest(string.Empty);
        }
    }
}