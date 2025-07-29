using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;

namespace Application.Mappings
{
    public class CompanyMappings : Profile
    {
        public CompanyMappings()
        {
            CreateMap<Company, CompanyDto>().ReverseMap();
        }
    }
}
