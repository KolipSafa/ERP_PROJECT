using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Queries
{
    public record GetAllCustomersQuery(
        bool IncludeInactive,
        string? SearchTerm,
        string? SortBy,
        bool IsDescending) : IRequest<IEnumerable<CustomerDto>>;

    public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllCustomersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _unitOfWork.CustomerRepository.GetAllAsync(
                request.IncludeInactive,
                request.SearchTerm,
                request.SortBy,
                request.IsDescending);

            var customerDtos = _mapper.Map<List<CustomerDto>>(customers);

            // TODO: Müşterilerin Supabase'deki hesap durumunu (EmailConfirmed)
            // Supabase Admin API'si üzerinden sorgulayarak `IsAccountActive` alanını doldur.
            // Bu işlem, birden fazla müşteri için toplu bir sorgu ile yapılmalıdır.
            foreach (var dto in customerDtos)
            {
                dto.IsAccountActive = false; // Şimdilik varsayılan olarak false ayarlıyoruz.
            }
            
            return customerDtos;
        }
    }
}
