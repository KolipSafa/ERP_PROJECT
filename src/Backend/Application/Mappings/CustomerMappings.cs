using Application.DTOs;
using Application.Features.Customers.Commands; // UpdateCustomerCommand için eklendi
using AutoMapper;
using Core.Domain.Entities;

namespace Application.Mappings
{
    public class CustomerMappings : Profile
    {
        public CustomerMappings()
        {
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null));
        }
    }
}
