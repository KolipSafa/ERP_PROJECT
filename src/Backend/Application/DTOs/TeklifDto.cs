using Core.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class TeklifDto
    {
        public Guid Id { get; set; }
        public string TeklifNumarasi { get; set; } = string.Empty;
        public Guid MusteriId { get; set; }
        public string MusteriAdi { get; set; } = string.Empty; // Frontend için müşteri adını ekliyoruz.
        public DateTime TeklifTarihi { get; set; }
        public DateTime GecerlilikTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public string Durum { get; set; } = string.Empty; // Enum'ı string'e çevirerek göndereceğiz.
        public bool IsActive { get; set; }
        public string? ChangeRequestNotes { get; set; }
        public List<TeklifSatiriDto> TeklifSatirlari { get; set; } = new List<TeklifSatiriDto>();
    }
}
