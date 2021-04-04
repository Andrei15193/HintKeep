using Microsoft.AspNetCore.Mvc;

namespace HintKeep.Controllers
{
    [ApiController, Route("api/users/details")]
    public class UsersDetailsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
            => NoContent();
    }
}