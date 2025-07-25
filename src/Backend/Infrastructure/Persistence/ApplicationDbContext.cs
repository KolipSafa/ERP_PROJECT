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
        }
    }
}