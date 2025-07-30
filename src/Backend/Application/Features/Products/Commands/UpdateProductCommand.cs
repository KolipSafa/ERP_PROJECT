using Application.DTOs;
using Application.Validators.Products;
using MediatR;

namespace Application.Features.Products.Commands
{
    // Nullable yapıyı koruyoruz ama arayüzleri uyguluyoruz.
    public record UpdateProductCommand : IRequest<ProductDto>, IProductName, IProductPrice, IProductStock, IProductSku
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? CurrencyId { get; set; }
        public int? StockQuantity { get; set; }
        public string? SKU { get; set; }
        public bool? IsActive { get; set; }
    }
}
