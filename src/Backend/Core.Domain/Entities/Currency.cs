using System;

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
    }
}
