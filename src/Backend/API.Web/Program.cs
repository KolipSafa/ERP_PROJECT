using API.Web.Middleware;
using Application.Common.Behaviors;
using Application.Mappings;
using Core.Domain.Interfaces;
using FluentValidation;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories; // Eklendi
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// CORS Politikasını Tanımla
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173") // Frontend'in adresi
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add FluentValidation and Validators
// Yaşam döngüsünü Scoped olarak ayarlıyoruz ki veritabanı erişimi yapabilelim.
builder.Services.AddValidatorsFromAssembly(typeof(ProductMappings).Assembly, ServiceLifetime.Scoped);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(ProductMappings).Assembly);

// Add Unit of Work and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>(); // Eklendi
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>(); // Eklendi


// Configure MediatR with Pipeline Behaviors
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ProductMappings).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

// CORS Politikasını Uygula
app.UseCors(MyAllowSpecificOrigins);

// app.UseHttpsRedirection(); // Will be re-enabled later

app.UseAuthorization();

app.MapControllers();

app.Run();
