using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly ApplicationDbContext _context;

        public CurrencyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Currency?> GetByIdAsync(int id)
        {
            return await _context.Currencies.FindAsync(id);
        }

        public async Task<IEnumerable<Currency>> GetAllAsync()
        {
            return await _context.Currencies.Where(c => c.IsActive).ToListAsync();
        }

        public void Add(Currency currency)
        {
            _context.Currencies.Add(currency);
        }

        public void Update(Currency currency)
        {
            _context.Currencies.Update(currency);
        }

        public void Delete(Currency currency)
        {
            // Bu bir soft delete işlemidir.
            currency.IsActive = false;
            _context.Currencies.Update(currency);
        }
    }
}
