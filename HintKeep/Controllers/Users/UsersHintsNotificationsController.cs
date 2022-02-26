using System;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Users
{
    [ApiController, Route("api/users/hints/notifications")]
    public class UsersHintsNotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersHintsNotificationsController(IMediator mediator)
            => _mediator = mediator;

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync(UserAccountRecovery userAccountRecovery)
        {
            await _mediator.Send(new UserRequestHintCommand(userAccountRecovery.Email));

            return Created(new Uri("/api/users/sessions", UriKind.Relative), null);
        }
    }
}