using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities
{
    public class TeklifSatiri
    {
        public Guid Id { get; set; }

        public Guid TeklifId { get; set; }
        [ForeignKey("TeklifId")]
        public Teklif Teklif { get; set; } = null!;

        public int UrunId { get; set; }
        [ForeignKey("UrunId")]
        public Product Urun { get; set; } = null!;

        public string Aciklama { get; set; } = string.Empty;
        public decimal Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal Toplam { get; set; }
    }
}
