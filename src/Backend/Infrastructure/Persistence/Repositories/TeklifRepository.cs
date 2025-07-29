using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            // Bu metot, IsActive durumuna bakmaksızın kaydı getirir.
            return await _context.Teklifler
                .Include(t => t.TeklifSatirlari)
                    .ThenInclude(s => s.Urun)
                .Include(t => t.Musteri)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Teklif>> GetAllAsync(
            Guid? musteriId,
            DateTime? baslangicTarihi,
            DateTime? bitisTarihi,
            QuoteStatus? durum,
            bool includeInactive,
            string? sortBy,
            string? sortOrder)
        {
            var query = _context.Teklifler.Include(t => t.Musteri).AsQueryable();

            // Filtreleme
            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }
            if (musteriId.HasValue)
            {
                query = query.Where(t => t.MusteriId == musteriId.Value);
            }
            if (durum.HasValue)
            {
                query = query.Where(t => t.Durum == durum.Value);
            }
            if (baslangicTarihi.HasValue)
            {
                query = query.Where(t => t.TeklifTarihi >= baslangicTarihi.Value);
            }
            if (bitisTarihi.HasValue)
            {
                var bitisTarihiSonu = bitisTarihi.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.TeklifTarihi <= bitisTarihiSonu);
            }

            // Sıralama
            var sortColumns = new Dictionary<string, Expression<Func<Teklif, object>>>
            {
                { "date", t => t.TeklifTarihi },
                { "customer", t => t.Musteri.FirstName },
                { "amount", t => t.ToplamTutar }
            };

            var sortByColumn = sortBy?.ToLowerInvariant() ?? "date";
            if (sortColumns.TryGetValue(sortByColumn, out var sortExpression))
            {
                query = sortOrder?.ToLowerInvariant() == "asc"
                    ? query.OrderBy(sortExpression)
                    : query.OrderByDescending(sortExpression);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public void Add(Teklif teklif)
        {
            _context.Teklifler.Add(teklif);
        }

        public void DeleteSatir(TeklifSatiri satir)
        {
            _context.TeklifSatirlari.Remove(satir);
        }

        public void Delete(Teklif teklif)
        {
            // Bu bir soft delete işlemidir.
            teklif.IsActive = false;
            _context.Teklifler.Update(teklif);
        }
    }
}

