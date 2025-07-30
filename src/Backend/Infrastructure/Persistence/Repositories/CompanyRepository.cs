using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company?> GetByIdAsync(Guid id)
        {
            return await _context.Companies.FindAsync(id);
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _context.Companies.Where(c => c.IsActive).ToListAsync();
        }

        public void Add(Company company)
        {
            _context.Companies.Add(company);
        }

        public void Update(Company company)
        {
            _context.Companies.Update(company);
        }

        public void Delete(Company company)
        {
            // Bu bir soft delete işlemidir.
            company.IsActive = false;
            _context.Companies.Update(company);
        }
    }
}
