using Microsoft.AspNetCore.Mvc;

namespace API.Web.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("/")]
        public IActionResult Root() => Ok(new { status = "OK" });
    }
}


