using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// Bu sınıf, 'dotnet ef' komutlarının tasarım zamanında (design-time) ApplicationDbContext'i
    /// doğru ConnectionString ile nasıl oluşturacağını bilmesini sağlar.
    /// Katmanlı bir mimaride, başlangıç projesi (API.Web) dışındaki bir projede (Infrastructure)
    /// DbContext bulunuyorsa, EF Core araçları appsettings.json'ı otomatik olarak bulamayabilir.
    /// Bu fabrika sınıfı, bu sorunu çözmek için standart bir desendir.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Bu fabrika Infrastructure projesinde olduğu için, API.Web projesinin yolunu
            // ona göre hesaplamamız gerekiyor. Bu yapı, komutun nereden çalıştırıldığından
            // bağımsız olarak daha güvenilirdir.
            var apiProjectDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../API.Web"));

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectDirectory)
                .AddJsonFile("appsettings.json")
                // appsettings.Development.json dosyasını da okumasını sağlıyoruz.
                // 'optional: true' sayesinde bu dosya olmasa bile hata vermez.
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Could not find a connection string named 'DefaultConnection'.");
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
