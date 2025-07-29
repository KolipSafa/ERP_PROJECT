using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities
{
    public class Teklif
    {
        public Guid Id { get; set; }

        public string TeklifNumarasi { get; set; } = string.Empty;

        public Guid MusteriId { get; set; }
        [ForeignKey("MusteriId")]
        public Customer Musteri { get; set; } = null!;

        public DateTime TeklifTarihi { get; set; }
        public DateTime GecerlilikTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; } = null!;
        public QuoteStatus Durum { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<TeklifSatiri> TeklifSatirlari { get; set; } = new List<TeklifSatiri>();
    }
}
