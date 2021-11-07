using System;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Users
{
    [ApiController, Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
            => _mediator = mediator;

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync(UserRegistration userRegistration)
        {
            await _mediator.Send(new RegisterUserCommand
            {
                Email = userRegistration.Email,
                Hint = userRegistration.Hint,
                Password = userRegistration.Password
            });

            return Created(new Uri("/api/users/confirmations", UriKind.Relative), null);
        }
    }
}