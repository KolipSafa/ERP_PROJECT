using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- TEŞHİS İÇİN GEÇİCİ ENDPOINT ---
        // Bu endpoint, giriş yapmış kullanıcının JWT'sinden ayrıştırılan tüm kimlik bilgilerini (claim) gösterir.
        // Bu, rollerin doğru bir şekilde eklenip eklenmediğini doğrulamamıza yardımcı olacaktır.
        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(claims);
        }
    }
}