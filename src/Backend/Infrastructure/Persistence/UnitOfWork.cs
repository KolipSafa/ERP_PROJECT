
// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\Infrastructure\Persistence\UnitOfWork.cs

using Core.Domain.Interfaces;
using Infrastructure.Persistence.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IProductRepository? _productRepository;
        private ICustomerRepository? _customerRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Bu "lazy loading" desenidir.
        // ProductRepository'ye ilk kez erişilmeye çalışıldığında, _productRepository'nin null olup olmadığını kontrol eder.
        // Eğer null ise, yeni bir ProductRepository oluşturur ve döndürür.
        // Sonraki erişimlerde, zaten oluşturulmuş olanı tekrar kullanır.
        // Bu, sadece ihtiyaç duyulan repository'lerin oluşturulmasını sağlar.
        public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_context);

        public ICustomerRepository CustomerRepository => _customerRepository ??= new CustomerRepository(_context);

        // CancellationToken'ı DbContext'e iletiyoruz.
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // IDisposable arayüzünü uyguluyoruz.
        // Bu, UnitOfWork'ün işi bittiğinde veritabanı bağlantısını (_context)
        // güvenli bir şekilde sonlandırmasını ve kaynakları serbest bırakmasını sağlar.
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
