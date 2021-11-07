using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers.Users
{
    [ApiController, Route("api/users/details")]
    public class UsersDetailsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
            => NoContent();
    }
}