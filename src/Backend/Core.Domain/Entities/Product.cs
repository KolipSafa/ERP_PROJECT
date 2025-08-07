using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CurrencyId { get; set; }
        public Currency? Currency { get; set; }
        public int StockQuantity { get; set; }
        public int ReservedQuantity { get; set; } = 0;
        public string? SKU { get; set; }
        public bool IsActive { get; set; } = true;
    }
}