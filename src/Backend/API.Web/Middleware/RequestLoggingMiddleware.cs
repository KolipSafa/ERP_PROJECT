using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API.Web.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Authentication middleware'inden sonra bu middleware çalışacak.
            // HttpContext.User nesnesinin durumunu kontrol edelim.
            var user = context.User;

            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var claims = user.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                if (claims.Any())
                {
                    _logger.LogDebug("Authenticated User Claims: {Claims}", string.Join(", ", claims));
                }
                else
                {
                    _logger.LogDebug("Authenticated user has NO claims.");
                }
            }
            else
            {
                _logger.LogDebug("User is NOT authenticated at this point.");
            }

            await _next(context);
        }
    }
}
