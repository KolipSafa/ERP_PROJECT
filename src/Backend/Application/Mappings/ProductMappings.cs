using AutoMapper;
using Application.DTOs;
using Core.Domain.Entities;

namespace Application.Mappings
{
    public class ProductMappings : Profile
    {
        public ProductMappings()
        {
            // Sadece Entity -> DTO dönüşümünü tanımlıyoruz.
            // Bu, en güvenli ve en kontrollü yaklaşımdır.
            CreateMap<Product, ProductDto>();
        }
    }
}
