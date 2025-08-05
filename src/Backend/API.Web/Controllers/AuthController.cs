using MediatR;
using Microsoft.AspNetCore.Mvc;

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

        // Bu Controller, gelecekte yetkilendirme ile ilgili özel endpoint'ler gerekirse diye boş olarak bırakılmıştır.
        // Mevcut durumda tüm kimlik doğrulama işlemleri Supabase ve Frontend tarafından yönetilmektedir.
    }
}
