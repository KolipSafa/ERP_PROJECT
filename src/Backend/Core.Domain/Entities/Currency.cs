using System;
using System.Collections.Generic;

namespace Core.Domain.Entities
{
    /// <summary>
    /// Sistemde kullanılacak para birimlerini temsil eder.
    /// </summary>
    public class Currency
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Örn: Türk Lirası
        public string Code { get; set; } = string.Empty; // Örn: TRY
        public string Symbol { get; set; } = string.Empty; // Örn: ₺
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Teklif> Teklifler { get; set; } = new List<Teklif>();
    }
}
