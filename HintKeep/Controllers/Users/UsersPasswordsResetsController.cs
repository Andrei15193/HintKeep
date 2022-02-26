using System;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Users
{
    [ApiController, Route("/api/users/passwords/resets")]
    public class UsersPasswordsResetsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersPasswordsResetsController(IMediator mediator)
            => _mediator = mediator;

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync(UserAccountRecovery userAccountRecovery)
        {
            await _mediator.Send(new UserRequestPasswordResetCommand(
                userAccountRecovery.Email
            ));

            return Created(new Uri("/api/users/passwords/resets", UriKind.Relative), null);
        }
    }
}