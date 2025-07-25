using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Dummy Product Verisi Ekleme (Data Seeding)
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop Pro X1", Description = "Yüksek performanslı dizüstü bilgisayar", Price = 32000, StockQuantity = 50, SKU = "LP-PRO-X1", IsActive = true },
                new Product { Id = 2, Name = "Kablosuz Mouse", Description = "Ergonomik ve hassas optik mouse", Price = 850, StockQuantity = 200, SKU = "MS-WL-001", IsActive = true },
                new Product { Id = 3, Name = "Mekanik Klavye", Description = "RGB aydınlatmalı, oyuncular için", Price = 2500, StockQuantity = 150, SKU = "KB-MECH-RGB", IsActive = true },
                new Product { Id = 4, Name = "4K Monitör 27\"", Description = "Canlı renkler ve net görüntüler", Price = 8999, StockQuantity = 80, SKU = "MON-4K-27", IsActive = true },
                new Product { Id = 5, Name = "USB-C Hub", Description = "8-in-1 bağlantı noktası adaptörü", Price = 1200, StockQuantity = 300, SKU = "HUB-USBC-81", IsActive = true },
                new Product { Id = 6, Name = "Harici SSD 1TB", Description = "Hızlı veri transferi için taşınabilir SSD", Price = 3500, StockQuantity = 120, SKU = "SSD-EXT-1TB", IsActive = true },
                new Product { Id = 7, Name = "Webcam 1080p", Description = "Görüntülü görüşmeler için Full HD kamera", Price = 1500, StockQuantity = 180, SKU = "WC-FHD-01", IsActive = false },
                new Product { Id = 8, Name = "Gaming Headset", Description = "7.1 Surround sesli oyuncu kulaklığı", Price = 2800, StockQuantity = 90, SKU = "HS-GM-71", IsActive = true },
                new Product { Id = 9, Name = "Akıllı Saat SE", Description = "Fitness takibi ve bildirimler", Price = 6500, StockQuantity = 250, SKU = "SW-SE-01", IsActive = true },
                new Product { Id = 10, Name = "Tablet 10\"", Description = "Eğlence ve iş için ideal tablet", Price = 7800, StockQuantity = 110, SKU = "TAB-10-STD", IsActive = true },
                new Product { Id = 11, Name = "Güç Kaynağı 750W", Description = "Modüler ve verimli PSU", Price = 2100, StockQuantity = 70, SKU = "PSU-750W-MD", IsActive = false },
                new Product { Id = 12, Name = "Ekran Kartı RTX 4070", Description = "Yeni nesil oyun ve grafik performansı", Price = 25000, StockQuantity = 40, SKU = "GPU-RTX-4070", IsActive = true },
                new Product { Id = 13, Name = "RAM 16GB DDR5", Description = "Yüksek hızlı bellek kiti (2x8GB)", Price = 1800, StockQuantity = 400, SKU = "RAM-16-DDR5", IsActive = true },
                new Product { Id = 14, Name = "Soğutucu Fan", Description = "Sessiz ve etkili işlemci soğutucusu", Price = 950, StockQuantity = 220, SKU = "FAN-CPU-SLNT", IsActive = true },
                new Product { Id = 15, Name = "Anakart Z790", Description = "En yeni işlemciler için ATX anakart", Price = 9200, StockQuantity = 60, SKU = "MB-Z790-ATX", IsActive = true }
            );

            // Yeni Müşteri Verisi Ekleme (Data Seeding)
            // HasData metodu için ID'lerin manuel olarak verilmesi gerekir.
            // Bu ID'ler, veritabanı her oluşturulduğunda aynı kalır.
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = Guid.Parse("857f3a2c-4ab7-4ffa-9855-08ddcabefce4"), FirstName = "Ahmet", LastName = "Yılmaz", CompanyName = "Yılmaz İnşaat", Email = "ahmet.yilmaz@example.com", PhoneNumber = "05321234567", Balance = 15250.75m, IsActive = true, CreatedDate = new DateTime(2025, 6, 25) },
                new Customer { Id = Guid.Parse("b6a95892-1d17-46f7-1919-08ddcb81e05d"), FirstName = "Ayşe", LastName = "Kaya", CompanyName = "Kaya Gıda Ltd.", Email = "ayse.kaya@example.com", PhoneNumber = "05422345678", Balance = 0m, IsActive = true, CreatedDate = new DateTime(2025, 6, 30) },
                new Customer { Id = Guid.Parse("c7b06982-2e28-47f8-2020-08ddcb81e05e"), FirstName = "Mehmet", LastName = "Demir", CompanyName = "Demir Teknoloji", Email = "mehmet.demir@example.com", PhoneNumber = "05553456789", Balance = 8500m, IsActive = false, CreatedDate = new DateTime(2025, 6, 15) },
                new Customer { Id = Guid.Parse("d8c17a83-3f39-48f9-2121-08ddcb81e05f"), FirstName = "Fatma", LastName = "Çelik", CompanyName = "Çelik Sanayi A.Ş.", Email = "fatma.celik@example.com", PhoneNumber = "05334567890", Balance = 120000.50m, IsActive = true, CreatedDate = new DateTime(2025, 7, 10) },
                new Customer { Id = Guid.Parse("e9d28b84-404a-49fa-2222-08ddcb81e060"), FirstName = "Mustafa", LastName = "Arslan", CompanyName = "Arslan Lojistik", Email = "mustafa.arslan@example.com", PhoneNumber = "05445678901", Balance = 0m, IsActive = true, CreatedDate = new DateTime(2025, 7, 15) },
                new Customer { Id = Guid.Parse("fae39c85-515b-4afb-2323-08ddcb81e061"), FirstName = "Zeynep", LastName = "Öztürk", CompanyName = "Öztürk Danışmanlık", Email = "zeynep.ozturk@example.com", PhoneNumber = "05356789012", Balance = 500.25m, IsActive = true, CreatedDate = new DateTime(2025, 7, 20) },
                new Customer { Id = Guid.Parse("0bf4ad86-626c-4bfc-2424-08ddcb81e062"), FirstName = "Hüseyin", LastName = "Aydın", CompanyName = "Aydın Tekstil", Email = "huseyin.aydin@example.com", PhoneNumber = "05547890123", Balance = 25000m, IsActive = true, CreatedDate = new DateTime(2025, 6, 5) },
                new Customer { Id = Guid.Parse("1cf5be87-737d-4c0d-2525-08ddcb81e063"), FirstName = "Elif", LastName = "Şahin", CompanyName = "Şahin Market", Email = "elif.sahin@example.com", PhoneNumber = "05368901234", Balance = 0m, IsActive = false, CreatedDate = new DateTime(2025, 5, 26) },
                new Customer { Id = Guid.Parse("2df6cf88-848e-4d1e-2626-08ddcb81e064"), FirstName = "İbrahim", LastName = "Koç", CompanyName = "Koç Otomotiv", Email = "ibrahim.koc@example.com", PhoneNumber = "05469012345", Balance = 7850.00m, IsActive = true, CreatedDate = new DateTime(2025, 7, 3) },
                new Customer { Id = Guid.Parse("3ef7df89-959f-4e2f-2727-08ddcb81e065"), FirstName = "Meryem", LastName = "Yıldız", CompanyName = "Yıldız Mobilya", Email = "meryem.yildiz@example.com", PhoneNumber = "05370123456", Balance = 1250.50m, IsActive = true, CreatedDate = new DateTime(2025, 7, 7) },
                new Customer { Id = Guid.Parse("4f08e08a-a6b0-4f30-2828-08ddcb81e066"), FirstName = "Ali", LastName = "Can", CompanyName = "Can Bilişim", Email = "ali.can@example.com", PhoneNumber = "05531234567", Balance = 0m, IsActive = true, CreatedDate = new DateTime(2025, 7, 13) },
                new Customer { Id = Guid.Parse("5f19f18b-b7c1-4041-2929-08ddcb81e067"), FirstName = "Sultan", LastName = "Doğan", CompanyName = "Doğan Medya", Email = "sultan.dogan@example.com", PhoneNumber = "05392345678", Balance = 9900m, IsActive = true, CreatedDate = new DateTime(2025, 7, 17) },
                new Customer { Id = Guid.Parse("6f2a028c-c8d2-4152-3030-08ddcb81e068"), FirstName = "Osman", LastName = "Kurt", CompanyName = "Kurt Güvenlik", Email = "osman.kurt@example.com", PhoneNumber = "05493456789", Balance = 4500m, IsActive = false, CreatedDate = new DateTime(2025, 4, 26) },
                new Customer { Id = Guid.Parse("7f3b138d-d9e3-4263-3131-08ddcb81e069"), FirstName = "Hatice", LastName = "Polat", CompanyName = "Polat Holding", Email = "hatice.polat@example.com", PhoneNumber = "05314567890", Balance = 500000m, IsActive = true, CreatedDate = new DateTime(2025, 5, 31) },
                new Customer { Id = Guid.Parse("8f4c248e-eaf4-4374-3232-08ddcb81e06a"), FirstName = "Yusuf", LastName = "Güneş", CompanyName = "Güneş Enerji", Email = "yusuf.gunes@example.com", PhoneNumber = "05515678901", Balance = 0m, IsActive = true, CreatedDate = new DateTime(2025, 7, 21) },
                new Customer { Id = Guid.Parse("9f5d358f-fbe5-4485-3333-08ddcb81e06b"), FirstName = "Emine", LastName = "Bulut", CompanyName = "Bulut Yazılım", Email = "emine.bulut@example.com", PhoneNumber = "05346789012", Balance = 18000m, IsActive = true, CreatedDate = new DateTime(2025, 7, 22) },
                new Customer { Id = Guid.Parse("af6e4690-0cf6-4596-3434-08ddcb81e06c"), FirstName = "Murat", LastName = "Özdemir", CompanyName = "Özdemir Metal", Email = "murat.ozdemir@example.com", PhoneNumber = "05437890123", Balance = 75000m, IsActive = true, CreatedDate = new DateTime(2025, 7, 23) },
                new Customer { Id = Guid.Parse("bf7f5791-1da7-46a7-3535-08ddcb81e06d"), FirstName = "Sibel", LastName = "Aksoy", CompanyName = "Aksoy Kozmetik", Email = "sibel.aksoy@example.com", PhoneNumber = "05368901234", Balance = 0m, IsActive = false, CreatedDate = new DateTime(2025, 3, 27) },
                new Customer { Id = Guid.Parse("cf806892-2eb8-47b8-3636-08ddcb81e06e"), FirstName = "Ramazan", LastName = "Çetin", CompanyName = "Çetin Emlak", Email = "ramazan.cetin@example.com", PhoneNumber = "05529012345", Balance = 2500.50m, IsActive = true, CreatedDate = new DateTime(2025, 7, 24) },
                new Customer { Id = Guid.Parse("df917993-3fc9-48c9-3737-08ddcb81e06f"), FirstName = "Yasemin", LastName = "Taş", CompanyName = "Taş Mermer", Email = "yasemin.tas@example.com", PhoneNumber = "05380123456", Balance = 3200m, IsActive = true, CreatedDate = new DateTime(2025, 7, 25) }
            );
        }
    }
}