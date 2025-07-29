
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
        private ITeklifRepository? _teklifRepository;
        private ICurrencyRepository? _currencyRepository;
        private ICompanyRepository? _companyRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_context);
        public ICustomerRepository CustomerRepository => _customerRepository ??= new CustomerRepository(_context);
        public ITeklifRepository TeklifRepository => _teklifRepository ??= new TeklifRepository(_context);
        public ICurrencyRepository CurrencyRepository => _currencyRepository ??= new CurrencyRepository(_context);
        public ICompanyRepository CompanyRepository => _companyRepository ??= new CompanyRepository(_context);

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
