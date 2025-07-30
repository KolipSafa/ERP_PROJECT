namespace Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public int StockQuantity { get; set; }
        public string? SKU { get; set; }
        public bool IsActive { get; set; }
    }
}
