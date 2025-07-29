using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.Settings.Companies.Queries
{
    public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, IEnumerable<CompanyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCompaniesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
        {
            var companies = await _unitOfWork.CompanyRepository.GetAllAsync();

            // Basit filtreleme ve arama (in-memory)
            if (request.IsActive.HasValue)
            {
                companies = companies.Where(c => c.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLowerInvariant();
                companies = companies.Where(c => 
                    c.Name.ToLowerInvariant().Contains(searchTerm) ||
                    (c.TaxNumber != null && c.TaxNumber.Contains(searchTerm))
                );
            }
            
            return _mapper.Map<IEnumerable<CompanyDto>>(companies.OrderBy(c => c.Name));
        }
    }
}
