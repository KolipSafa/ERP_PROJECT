using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;

namespace Application.Mappings
{
    public class CurrencyMappings : Profile
    {
        public CurrencyMappings()
        {
            CreateMap<Currency, CurrencyDto>().ReverseMap();
        }
    }
}
