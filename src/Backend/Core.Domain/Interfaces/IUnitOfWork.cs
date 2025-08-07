
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository ProductRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        ITeklifRepository TeklifRepository { get; }
        ICurrencyRepository CurrencyRepository { get; }
        ICompanyRepository CompanyRepository { get; }
        IInvoiceRepository InvoiceRepository { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
