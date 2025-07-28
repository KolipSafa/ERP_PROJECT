using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    /// <summary>
    /// Teklif entity'si için veritabanı operasyonlarını tanımlayan arayüz.
    /// Proje mimarisine uygun olarak, okuma işlemleri asenkron (Task),
    /// yazma işlemleri ise Unit of Work deseni gereği senkrondur (void).
    /// </summary>
    public interface ITeklifRepository
    {
        /// <summary>
        /// Belirtilen ID'ye sahip teklifi, ilişkili satırları ile birlikte getirir.
        /// </summary>
        Task<Teklif?> GetByIdAsync(Guid id);

        /// <summary>
        /// Tüm teklifleri getirir. Gelecekte bu metoda filtreleme, sıralama ve sayfalama
        /// yetenekleri eklenecektir, tıpkı IProductRepository'de olduğu gibi.
        /// </summary>
        Task<IEnumerable<Teklif>> GetAllAsync();

        /// <summary>
        /// Yeni bir teklifi veritabanına eklenmek üzere işaretler.
        /// Kaydetme işlemi IUnitOfWork.SaveChangesAsync() ile yapılır.
        /// </summary>
        void Add(Teklif teklif);

        /// <summary>
        /// Mevcut bir teklifi güncellenmek üzere işaretler.
        /// Kaydetme işlemi IUnitOfWork.SaveChangesAsync() ile yapılır.
        /// </summary>
        void Update(Teklif teklif);

        /// <summary>
        /// Mevcut bir teklifi silinmek üzere işaretler.
        /// Bu genellikle 'IsActive' alanını false yapmak (soft delete) için kullanılır.
        /// Kaydetme işlemi IUnitOfWork.SaveChangesAsync() ile yapılır.
        /// </summary>
        void Delete(Teklif teklif);
    }
}
