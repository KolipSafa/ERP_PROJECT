using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Company?> GetByIdAsync(Guid id);
        Task<IEnumerable<Company>> GetAllAsync();
        void Add(Company company);
        void Update(Company company);
        void Delete(Company company);
    }
}
