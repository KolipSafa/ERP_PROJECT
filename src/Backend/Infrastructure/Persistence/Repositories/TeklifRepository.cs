using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class TeklifRepository : ITeklifRepository
    {
        private readonly ApplicationDbContext _context;

        public TeklifRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Teklif?> GetByIdAsync(Guid id)
        {
            // Bir teklifi getirirken, ona ait satırları, müşteriyi ve satırlardaki ürünleri de
            // tek bir sorguda getirmek için Include ve ThenInclude kullanıyoruz.
            // Bu, "N+1" sorgu problemini önler ve performansı artırır.
            return await _context.Teklifler
                .Include(t => t.TeklifSatirlari)
                    .ThenInclude(s => s.Urun)
                .Include(t => t.Musteri)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Teklif>> GetAllAsync()
        {
            // Tüm teklifleri, müşteri bilgileriyle birlikte getiriyoruz.
            // Liste görünümünde satır detayları ilk başta gerekmeyebilir, bu yüzden
            // burada satırları Include etmiyoruz. Bu, sorguyu daha hafif tutar.
            return await _context.Teklifler
                .Include(t => t.Musteri)
                .ToListAsync();
        }

        public void Add(Teklif teklif)
        {
            _context.Teklifler.Add(teklif);
        }

        public void Update(Teklif teklif)
        {
            _context.Teklifler.Update(teklif);
        }

        public void Delete(Teklif teklif)
        {
            // Genellikle bu metot soft delete için kullanılır, yani IsActive = false yapılır.
            // Gerçek silme işlemi yerine Update'i çağırıyoruz.
            // Gerçek silme (hard delete) istenirse ayrı bir metot (örn: HardDelete) yazılır.
            _context.Teklifler.Update(teklif);
        }
    }
}
