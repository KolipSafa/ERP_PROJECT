// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\Application\Features\Customers\Queries\GetAllCustomersQuery.cs

using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Queries
{
    // Query'miz artık filtreleme parametrelerini taşıyor.
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
            // Gelen parametreleri doğrudan repository metoduna iletiyoruz.
            var customers = await _unitOfWork.CustomerRepository.GetAllAsync(
                request.IncludeInactive,
                request.SearchTerm,
                request.SortBy,
                request.IsDescending);
            
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }
    }
}
