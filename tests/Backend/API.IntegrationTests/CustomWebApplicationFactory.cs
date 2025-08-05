using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;
using Application.Interfaces;

namespace API.IntegrationTests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                // Mevcut DbContext yapılandırmasını kaldır (varsa)
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Testler için SQLite veritabanını ekle
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    var connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();
                    options.UseSqlite(connection);
                });

                // IBackgroundJobService için sahte servis ekle
                var backgroundJobServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBackgroundJobService));
                if (backgroundJobServiceDescriptor != null)
                {
                    services.Remove(backgroundJobServiceDescriptor);
                }
                services.AddScoped<IBackgroundJobService, MockBackgroundJobService>();
            });
        }
    }
}