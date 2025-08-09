using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Headers;
using API.Web.Configuration;

namespace API.Web.Middleware
{
    // JWT doğrulama 401 verirse, Supabase /auth/v1/user endpoint'i ile token'ı uzaktan doğrulayıp
    // HttpContext.User'ı elle oluşturur. Böylece [Authorize] sorunsuz çalışır.
    public class SupabaseRemoteAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _authority;
        private readonly string _apiKey;
        private readonly ILogger<SupabaseRemoteAuthMiddleware> _logger;

        public SupabaseRemoteAuthMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<SupabaseRemoteAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
            _authority = jwt.Authority.TrimEnd('/');
            _apiKey = configuration["Supabase:ServiceRoleKey"]
                      ?? configuration["Supabase:AnonKey"]
                      ?? string.Empty;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Mevcutta kimlik yoksa ve Authorization: Bearer varsa, uzaktan doğrulamayı dene
            if ((context.User?.Identity?.IsAuthenticated ?? false) == false)
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    try
                    {
                        using var http = new HttpClient();
                        var req = new HttpRequestMessage(HttpMethod.Get, $"{_authority}/user");
                        if (!string.IsNullOrWhiteSpace(_apiKey))
                        {
                            req.Headers.Add("apikey", _apiKey);
                            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                        else
                        {
                            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }

                        var resp = await http.SendAsync(req);
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            using var doc = JsonDocument.Parse(json);
                            var root = doc.RootElement;
                            // Beklenen şema: { user: { id, email, app_metadata: { roles: [] } } } veya doğrudan { id, ... }
                            var userElem = root.TryGetProperty("user", out var u) ? u : root;
                            var userId = userElem.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                            var email = userElem.TryGetProperty("email", out var em) ? em.GetString() : null;
                            var roles = new List<string>();
                            if (userElem.TryGetProperty("app_metadata", out var am) && am.ValueKind == JsonValueKind.Object)
                            {
                                if (am.TryGetProperty("roles", out var rolesArr) && rolesArr.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var r in rolesArr.EnumerateArray())
                                    {
                                        var rv = r.GetString();
                                        if (!string.IsNullOrWhiteSpace(rv)) roles.Add(rv!);
                                    }
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(userId))
                            {
                                var claims = new List<Claim>
                                {
                                    new Claim("sub", userId!),
                                    new Claim(ClaimTypes.NameIdentifier, userId!)
                                };
                                if (!string.IsNullOrWhiteSpace(email)) claims.Add(new Claim(ClaimTypes.Email, email!));
                                foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

                                var identity = new ClaimsIdentity(claims, "SupabaseRemote");
                                context.User = new ClaimsPrincipal(identity);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Supabase remote auth doğrulaması sırasında hata oluştu.");
                    }
                }
            }

            await _next(context);
        }
    }
}


