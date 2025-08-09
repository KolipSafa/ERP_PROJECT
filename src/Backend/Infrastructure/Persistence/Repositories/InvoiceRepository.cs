using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetByIdAsync(Guid id)
        {
            return await _context.Invoices
                .Include(i => i.InvoiceLines)
                .ThenInclude(l => l.Product)
                .Include(i => i.Customer)!.ThenInclude(c => c.Company)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Invoice>> GetAllByCustomerIdAsync(Guid customerId)
        {
            return await _context.Invoices
                .Include(i => i.Customer)!.ThenInclude(c => c.Company)
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public void Add(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.InvoiceLines)
                .Include(i => i.Customer)!.ThenInclude(c => c.Company)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }
    }
}
