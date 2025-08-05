using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            
            // Proje kökünü bulana kadar yukarı çık (örneğin, .git klasörünü arayarak)
            while (basePath != null && !Directory.Exists(Path.Combine(basePath, ".git")))
            {
                basePath = Directory.GetParent(basePath)?.FullName;
            }

            if (string.IsNullOrEmpty(basePath))
            {
                throw new InvalidOperationException("Could not find the project root directory.");
            }

            var apiProjectDirectory = Path.Combine(basePath, "src", "Backend", "API.Web");

            if (!Directory.Exists(apiProjectDirectory))
            {
                throw new InvalidOperationException($"API project directory not found at: {apiProjectDirectory}");
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Could not find a connection string named 'DefaultConnection' in the configuration files.");
            }

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
