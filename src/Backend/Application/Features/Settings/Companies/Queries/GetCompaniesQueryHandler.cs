using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Settings.Companies.Queries;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, IEnumerable<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;

    public GetCompaniesQueryHandler(ICompanyRepository companyRepository, IMapper mapper)
    {
        _companyRepository = companyRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _companyRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CompanyDto>>(companies);
    }
}