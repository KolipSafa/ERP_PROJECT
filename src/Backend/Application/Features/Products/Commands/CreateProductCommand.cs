using Application.DTOs;
using Application.Validators.Products;
using MediatR;

namespace Application.Features.Products.Commands
{
    // Price ve StockQuantity'yi nullable yaparak arayüzle uyumlu hale getiriyoruz.
    public record CreateProductCommand : IRequest<ProductDto>, IProductName, IProductPrice, IProductStock
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? StockQuantity { get; set; }
    }
}
