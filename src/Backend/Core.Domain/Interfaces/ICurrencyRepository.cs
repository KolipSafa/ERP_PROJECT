using Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface ICurrencyRepository
    {
        Task<Currency?> GetByIdAsync(int id);
        Task<IEnumerable<Currency>> GetAllAsync();
        void Add(Currency currency);
        void Update(Currency currency);
        void Delete(Currency currency);
    }
}
