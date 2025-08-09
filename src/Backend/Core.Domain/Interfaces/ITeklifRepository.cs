using Core.Domain.Entities;
using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    /// <summary>
    /// Teklif entity'si için veritabanı operasyonlarını tanımlayan arayüz.
    /// </summary>
    public interface ITeklifRepository
    {
        /// <summary>
        /// Belirtilen ID'ye sahip teklifi, ilişkili satırları ile birlikte getirir.
        /// </summary>
        Task<Teklif?> GetByIdAsync(Guid id);

        /// <summary>
        /// Belirtilen kriterlere göre filtrelenmiş ve sıralanmış tüm teklifleri getirir.
        /// </summary>
        Task<IEnumerable<Teklif>> GetAllAsync(
            Guid? musteriId,
            Guid? applicationUserId,
            DateTime? baslangicTarihi,
            DateTime? bitisTarihi,
            QuoteStatus? durum,
            bool includeInactive,
            string? sortBy,
            string? sortOrder
        );

        /// <summary>
        /// Yeni bir teklifi veritabanına eklenmek üzere işaretler.
        /// Kaydetme işlemi IUnitOfWork.SaveChangesAsync() ile yapılır.
        /// </summary>
        void Add(Teklif teklif);

        /// <summary>
        /// Veritabanından bir teklif satırını silinmek üzere işaretler.
        /// </summary>
        void DeleteSatir(TeklifSatiri satir);

        /// <summary>
        /// Mevcut bir teklifi silinmek üzere işaretler.
        /// Bu genellikle 'IsActive' alanını false yapmak (soft delete) için kullanılır.
        /// Kaydetme işlemi IUnitOfWork.SaveChangesAsync() ile yapılır.
        /// </summary>
        void Delete(Teklif teklif);

        /// <summary>
        /// İlgili teklifi ve satırlarını veritabanından kalıcı olarak siler.
        /// </summary>
        void HardDelete(Teklif teklif);
    }
}
