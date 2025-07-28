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
                .ForMember(dest => dest.UrunAdi, opt => opt.MapFrom(src => src.Urun.Name));

            // Teklif'ten TeklifDto'ya haritalama
            CreateMap<Teklif, TeklifDto>()
                .ForMember(
                    dest => dest.MusteriAdi, 
                    opt => opt.MapFrom(src => $"{src.Musteri.FirstName} {src.Musteri.LastName}")
                )
                .ForMember(
                    dest => dest.Durum, 
                    opt => opt.MapFrom(src => src.Durum.ToString())
                );
        }
    }
}
