using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;

namespace Application.Mappings
{
    public class TeklifMappings : Profile
    {
        public TeklifMappings()
        {
            // TeklifSatiri'ndan TeklifSatiriDto'ya haritalama
            CreateMap<TeklifSatiri, TeklifSatiriDto>()
                .ForMember(dest => dest.UrunAdi, opt => opt.MapFrom(src => src.Urun != null ? src.Urun.Name : string.Empty));

            // Teklif'ten TeklifDto'ya haritalama
            CreateMap<Teklif, TeklifDto>()
                .ForMember(
                    dest => dest.MusteriAdi, 
                    opt => opt.MapFrom(src => src.Musteri != null ? $"{src.Musteri.FirstName} {src.Musteri.LastName}" : string.Empty)
                )
                .ForMember(
                    dest => dest.CurrencyCode,
                    opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Code : null)
                )
                .ForMember(
                    dest => dest.Durum, 
                    opt => opt.MapFrom(src => src.Durum.ToString())
                );

            // Invoice -> InvoiceDto (tek tanım)
            CreateMap<Invoice, InvoiceDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (int)s.Status))
                .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer != null ? ($"{s.Customer.FirstName} {s.Customer.LastName}") : null))
                .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.Customer != null && s.Customer.Company != null ? s.Customer.Company.Name : null));
        }
    }
}
