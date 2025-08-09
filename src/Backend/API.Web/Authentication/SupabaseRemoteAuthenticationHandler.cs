using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using API.Web.Configuration;

namespace API.Web.Authentication
{
    public class SupabaseRemoteAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly JwtSettings _jwtSettings;
        private readonly string _apiKey;

        public SupabaseRemoteAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            _jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
            _apiKey = configuration["Supabase:ServiceRoleKey"]
                      ?? configuration["Supabase:AnonKey"]
                      ?? string.Empty;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.NoResult();
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.NoResult();
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return AuthenticateResult.NoResult();
            }

            try
            {
                using var http = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_jwtSettings.Authority.TrimEnd('/')}/user");
                if (!string.IsNullOrWhiteSpace(_apiKey))
                {
                    request.Headers.Add("apikey", _apiKey);
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return AuthenticateResult.NoResult();
                }

                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var userElem = root.TryGetProperty("user", out var u) ? u : root;

                var userId = userElem.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var email = userElem.TryGetProperty("email", out var em) ? em.GetString() : null;
                var claims = new List<Claim>();
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    claims.Add(new Claim("sub", userId!));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId!));
                }
                if (!string.IsNullOrWhiteSpace(email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, email!));
                }
                if (userElem.TryGetProperty("app_metadata", out var am) && am.ValueKind == JsonValueKind.Object)
                {
                    if (am.TryGetProperty("roles", out var rolesArr) && rolesArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var r in rolesArr.EnumerateArray())
                        {
                            var rv = r.GetString();
                            if (!string.IsNullOrWhiteSpace(rv))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, rv!));
                            }
                        }
                    }
                }

                if (claims.Count == 0)
                {
                    return AuthenticateResult.NoResult();
                }

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SupabaseRemoteAuthenticationHandler error");
                return AuthenticateResult.Fail(ex);
            }
        }
    }
}


