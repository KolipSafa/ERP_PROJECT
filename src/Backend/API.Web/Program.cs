using API.Web.Configuration;
using API.Web.Middleware;
using Application.Common.Behaviors;
using Application.Interfaces;
using Application.Mappings;
using Core.Domain.Interfaces;
using FluentValidation;
using Hangfire;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Hangfire.PostgreSql;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using API.Web.Authentication;
using Microsoft.AspNetCore.Authentication;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Force Kestrel to bind to Render's PORT (default 10000)
var renderPort = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
Log.Information("Binding Kestrel to http://0.0.0.0:{Port}", renderPort);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_myAllowSpecificOrigins", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin)) return false;
                if (origin.StartsWith("http://localhost:5173") || origin.StartsWith("https://localhost:5173")) return true;
                try
                {
                    var host = new Uri(origin).Host.ToLowerInvariant();
                    return host.EndsWith(".vercel.app");
                }
                catch
                {
                    return false;
                }
            })
            .AllowCredentials()
            .WithHeaders("authorization", "content-type")
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });
});

// Default off in Production; enable explicitly via Hangfire:Enabled=true
var hangfireEnabled = builder.Configuration.GetValue<bool>("Hangfire:Enabled", false);
// Auto-migrate DB only in Development by default (can enable via Database:AutoMigrate=true)
var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate", builder.Environment.IsDevelopment());

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    if (hangfireEnabled)
    {
        builder.Services.AddHangfire(config => config
            .UsePostgreSqlStorage(options => 
                options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
        builder.Services.AddHangfireServer();
    }
}

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Authority) || string.IsNullOrEmpty(jwtSettings.Audience))
{
    throw new InvalidOperationException("JWT settings are not configured in appsettings.");
}

// Supabase API key (service_role tercih edilir; yoksa anon key)
var supabaseApiKey = builder.Configuration["Supabase:ServiceRoleKey"]
                      ?? builder.Configuration["Supabase:AnonKey"]
                      ?? string.Empty;

// Top-level local function: Token doğrulama parametrelerini HS256 (SigningKey) veya RS256 (JWKS) ile kurar
TokenValidationParameters BuildTokenValidationParameters(JwtSettings settings)
{
    var parameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = settings.Authority,
        ValidateAudience = true,
        ValidAudience = settings.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };

    if (!string.IsNullOrWhiteSpace(settings.SigningKey))
    {
        // Supabase Legacy JWT Secret genellikle Base64 string olarak verilir
        // Base64 decode başarısız olursa düz metin olarak kullan
        byte[] keyBytes;
        try
        {
            keyBytes = Convert.FromBase64String(settings.SigningKey);
        }
        catch
        {
            keyBytes = System.Text.Encoding.UTF8.GetBytes(settings.SigningKey);
        }
        var symmetricKey = new SymmetricSecurityKey(keyBytes);
        // IssuerSigningKey(ler)i doğrudan ata
        parameters.IssuerSigningKey = symmetricKey;
        parameters.IssuerSigningKeys = new SecurityKey[] { symmetricKey };
        return parameters;
    }

    parameters.IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
    {
        var urls = new[]
        {
            $"{settings.Authority}/keys",
            $"{settings.Authority}/jwks",
            $"{settings.Authority}/.well-known/jwks.json"
        };
        foreach (var url in urls)
        {
            try
            {
                using var http = new System.Net.Http.HttpClient();
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                if (!string.IsNullOrWhiteSpace(supabaseApiKey))
                {
                    request.Headers.Add("apikey", supabaseApiKey);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", supabaseApiKey);
                }
                var response = http.Send(request);
                if (!response.IsSuccessStatusCode) continue;
                var jwksJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var jwks = new JsonWebKeySet(jwksJson);
                var keys = jwks.GetSigningKeys();
                if (keys?.Count > 0) return keys;
            }
            catch { /* try next */ }
        }
        return Array.Empty<SecurityKey>();
    };

    return parameters;
}

builder.Services.AddAuthentication(options =>
{
    // Varsayılan kimlik doğrulama şemasını SupabaseRemote yapıyoruz.
    // Böylece [Authorize] çağrılarında JWKS başarısız olsa bile /auth/v1/user doğrulaması devreye girer.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.Authority = jwtSettings.Authority; // Supabase Auth base
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = BuildTokenValidationParameters(jwtSettings);

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JWT");
            logger.LogError(context.Exception, "JWT doğrulama hatası");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity &&
                (context.Principal.HasClaim(c => c.Type == "app_metadata") || context.Principal.HasClaim(c => c.Type == "raw_app_meta_data")))
            {
                var appMetadata = context.Principal.Claims.FirstOrDefault(c => c.Type == "app_metadata")?.Value
                                  ?? context.Principal.Claims.FirstOrDefault(c => c.Type == "raw_app_meta_data")?.Value;
                if (!string.IsNullOrEmpty(appMetadata))
                {
                    using var jsonDoc = System.Text.Json.JsonDocument.Parse(appMetadata);
                    if (jsonDoc.RootElement.TryGetProperty("roles", out var rolesElem))
                    {
                        foreach (var role in rolesElem.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (roleValue != null)
                            {
                                identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    };
});

// Fallback yolu: JWT başarısız olursa middleware Supabase /auth/v1/user ile doğrulayıp context.User set eder.

builder.Services.AddHttpClient<ISupabaseAuthAdminService, SupabaseAuthAdminService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITeklifRepository, TeklifRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IBackgroundJobService, HangfireJobService>();
builder.Services.AddAutoMapper(typeof(ProductMappings).Assembly);
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ProductMappings).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Prod ortamında Swagger'ı kapat (güvenlik)
}

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("_myAllowSpecificOrigins");

app.UseAuthentication();
// Fallback: JWT doğrulama başarısız olsa bile Supabase /auth/v1/user ile kimliği üret
app.UseMiddleware<SupabaseRemoteAuthMiddleware>();
// Tanı için: kimlik ve claim'leri logla (yalnızca Development)
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<RequestLoggingMiddleware>();
}
app.UseAuthorization();

app.MapControllers();

if (!app.Environment.IsEnvironment("Test"))
{
    if (hangfireEnabled && app.Environment.IsDevelopment())
    {
        try
        {
            app.MapHangfireDashboard();
        }
        catch (Exception ex)
        {
            var errorLogger = app.Services.GetRequiredService<ILogger<Program>>();
            errorLogger.LogError(ex, "Hangfire dashboard başlatılamadı. Geliştirme için skip ediliyor.");
        }
    }

    if (autoMigrate)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var errorLogger = services.GetRequiredService<ILogger<Program>>();
            errorLogger.LogError(ex, "Veritabanı migration sırasında bir hata oluştu.");
        }
    }
}

app.Run();

public partial class Program { }
