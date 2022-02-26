using System;
using System.Threading.Tasks;
using HintKeep.RequestsHandlers.Users.Commands;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Users
{
    [ApiController, Route("api/users/sessions")]
    public class UsersSessionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersSessionsController(IMediator mediator)
            => _mediator = mediator;

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync(UserAuthentication userAuthentication)
        {
            var jsonWebToken = await _mediator.Send(new CreateUserSessionCommand(
                userAuthentication.Email,
                userAuthentication.Password
            ));

            return Created(new Uri("/api/users/sessions", UriKind.Relative), jsonWebToken);
        }
    }
}