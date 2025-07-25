using Application.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Products.Queries
{
    public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>
    {
        // Filtreleme
        public string? Search { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool IncludeInactive { get; set; } = false;

        // Sıralama
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } // "asc" veya "desc"
    }
}
