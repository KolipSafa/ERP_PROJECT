using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET /api/health/env
        // Debug amaçlı: kritik env'lerin bindiğini doğrular (gizli değerleri göstermez)
        [HttpGet("env")]
        public IActionResult Env()
        {
            // Connection string'i sadece var/yok ve host/port olarak maskeleriz
            var conn = _configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            string? host = null, port = null;
            if (!string.IsNullOrWhiteSpace(conn))
            {
                foreach (var part in conn.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = part.Split('=', 2);
                    if (kv.Length != 2) continue;
                    var key = kv[0].Trim().ToLowerInvariant();
                    var val = kv[1].Trim();
                    if (key == "host" || key == "server") host = val;
                    if (key == "port") port = val;
                }
            }

            var payload = new
            {
                aspnetcoreEnvironment = _configuration["ASPNETCORE_ENVIRONMENT"],
                jwtAuthority = _configuration["Jwt:Authority"] ?? _configuration["Jwt__Authority"],
                jwtAudience = _configuration["Jwt:Audience"] ?? _configuration["Jwt__Audience"],
                supabaseUrl = _configuration["Supabase:Url"] ?? _configuration["Supabase__Url"],
                hangfireEnabled = _configuration.GetValue<bool?>("Hangfire:Enabled") ?? _configuration.GetValue<bool?>("Hangfire__Enabled"),
                hasConnectionString = !string.IsNullOrWhiteSpace(conn),
                dbHost = host,
                dbPort = port
            };

            return Ok(payload);
        }
    }
}


