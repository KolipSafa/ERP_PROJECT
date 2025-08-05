using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Teklif> Teklifler { get; set; }
        public DbSet<TeklifSatiri> TeklifSatirlari { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Administrator> Administrators { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal türleri için hassasiyet ve ölçek belirleme
            modelBuilder.Entity<Customer>().Property(c => c.Balance).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Teklif>().Property(t => t.ToplamTutar).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<TeklifSatiri>().Property(ts => ts.BirimFiyat).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<TeklifSatiri>().Property(ts => ts.Miktar).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<TeklifSatiri>().Property(ts => ts.Toplam).HasColumnType("decimal(18, 2)");

            // Customer - Email unique index (PostgreSQL uyumlu)
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique()
                .HasFilter(@"""Email"" IS NOT NULL");

            // TeklifSatiri - Product relationship (Fix for multiple cascade paths)
            // Bir ürün silinirse, geçmiş tekliflerdeki satırları silinmemelidir.
            modelBuilder.Entity<TeklifSatiri>()
                .HasOne(ts => ts.Urun)
                .WithMany()
                .HasForeignKey(ts => ts.UrunId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir şirket silinirse, o şirkete bağlı müşteriler silinmemelidir.
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Company)
                .WithMany(co => co.Customers)
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir para birimi silinirse, o para birimini kullanan ürünler silinmemelidir.
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Currency)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir para birimi silinirse, o para birimini kullanan teklifler silinmemelidir.
            modelBuilder.Entity<Teklif>()
                .HasOne(t => t.Currency)
                .WithMany(c => c.Teklifler)
                .HasForeignKey(t => t.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir müşteri silinirse, o müşteriye ait teklifler silinmemelidir.
            modelBuilder.Entity<Teklif>()
                .HasOne(t => t.Musteri)
                .WithMany()
                .HasForeignKey(t => t.MusteriId)
                .OnDelete(DeleteBehavior.Restrict);

            // Data Seeding for Currencies
            modelBuilder.Entity<Currency>().HasData(
                new Currency { Id = 1, Name = "Türk Lirası", Code = "TRY", Symbol = "₺", IsActive = true },
                new Currency { Id = 2, Name = "Amerikan Doları", Code = "USD", Symbol = "$", IsActive = true },
                new Currency { Id = 3, Name = "Euro", Code = "EUR", Symbol = "€", IsActive = true }
            );
        }
    }
}
