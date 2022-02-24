using System;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.ViewModels.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Users
{
    [ApiController, Route("/api/users/passwords")]
    public class UsersPasswordsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersPasswordsController(IMediator mediator)
            => _mediator = mediator;

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PostAsync(UserPasswordReset userPasswordReset)
        {
            await _mediator.Send(new UserPasswordResetCommand
            {
                Email = userPasswordReset.Email,
                Token = userPasswordReset.Token,
                Password = userPasswordReset.Password
            });
            return Created(new Uri("/api/users/sessions", UriKind.Relative), null);
        }
    }
}