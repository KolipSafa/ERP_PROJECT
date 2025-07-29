// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\Infrastructure\Persistence\Repositories\CustomerRepository.cs

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
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(
            bool includeInactive = false,
            string? searchTerm = null,
            string? sortBy = null,
            bool isDescending = false)
        {
            var query = _context.Customers.Include(c => c.Company).AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(term) ||
                    c.LastName.ToLower().Contains(term) ||
                    (c.Company != null && c.Company.Name.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term))
                );
            }

            query = sortBy?.ToLower() switch
            {
                "name" => isDescending
                    ? query.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName)
                    : query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName),
                "company" => isDescending
                    ? query.OrderByDescending(c => c.Company.Name)
                    : query.OrderBy(c => c.Company.Name),
                "date" => isDescending
                    ? query.OrderByDescending(c => c.CreatedDate)
                    : query.OrderBy(c => c.CreatedDate),
                _ => isDescending
                    ? query.OrderByDescending(c => c.CreatedDate)
                    : query.OrderBy(c => c.CreatedDate)
            };

            return await query.ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        }

        public void Add(Customer customer)
        {
            _context.Customers.Add(customer);
        }

        public void Update(Customer customer)
        {
            customer.UpdatedDate = DateTime.UtcNow;
            _context.Customers.Update(customer);
        }

        public void Delete(Customer customer)
        {
            // Bu bir soft delete işlemidir.
            customer.IsActive = false;
            customer.UpdatedDate = DateTime.UtcNow;
            _context.Customers.Update(customer);
        }
    }
}