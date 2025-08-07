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

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_myAllowSpecificOrigins",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Use .NET 9's built-in OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });
});


if (builder.Environment.IsEnvironment("Test") == false)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
    
    // Correctly configure Hangfire with a connection string
    builder.Services.AddHangfire(config => config
        .UsePostgreSqlStorage(options => 
        {
            options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
        }));
    builder.Services.AddHangfireServer();
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Jwt:Authority"];
    options.Audience = builder.Configuration["Jwt:Audience"];
    options.RequireHttpsMetadata = false;

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Supabase JWT'sindeki rolleri ayrıştırıp .NET kimliğine ekliyoruz.
            if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity &&
                context.Principal.HasClaim(c => c.Type == "raw_app_meta_data"))
            {
                var appMetadata = context.Principal.Claims
                    .FirstOrDefault(c => c.Type == "raw_app_meta_data")?.Value;

                if (!string.IsNullOrEmpty(appMetadata))
                {
                    using (var jsonDoc = System.Text.Json.JsonDocument.Parse(appMetadata))
                    {
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
            }
            return Task.CompletedTask;
        }
    };
});

// Add HttpClient for our custom Supabase service and register the service
builder.Services.AddHttpClient<ISupabaseAuthAdminService, SupabaseAuthAdminService>();


// Add HttpClient for our custom Supabase service and register the service
builder.Services.AddHttpClient<ISupabaseAuthAdminService, SupabaseAuthAdminService>();


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITeklifRepository, TeklifRepository>();

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
    // Correctly use the built-in OpenAPI middleware
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("_myAllowSpecificOrigins");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


if (app.Environment.IsEnvironment("Test") == false)
{
    app.MapHangfireDashboard();
    
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Veritabanı seed işlemi sırasında bir hata oluştu.");
        }
    }
}


app.Run();

public partial class Program { }
