using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id);
        Task<IEnumerable<Customer>> GetAllAsync(
            bool includeInactive = false,
            string? searchTerm = null,
            string? sortBy = null,
            bool isDescending = false);
        Task<Customer?> GetByEmailAsync(string? email);
        void Add(Customer customer);
        void Update(Customer customer);
        void Delete(Customer customer);
    }
}
