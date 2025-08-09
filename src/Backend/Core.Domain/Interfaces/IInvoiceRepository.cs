using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(Guid id);
        Task<IEnumerable<Invoice>> GetAllByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Invoice>> GetAllAsync();
        void Add(Invoice invoice);
    }
}
