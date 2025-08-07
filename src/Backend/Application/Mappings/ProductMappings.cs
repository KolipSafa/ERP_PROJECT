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
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Code : null))
                .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.StockQuantity - src.ReservedQuantity));
        }
    }
}
