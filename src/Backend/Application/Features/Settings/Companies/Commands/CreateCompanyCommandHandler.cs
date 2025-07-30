using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Settings.Companies.Commands;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = new Company
        {
            Name = request.Name,
            TaxNumber = request.TaxNumber,
            Address = request.Address,
            IsActive = true // Yeni eklenenler varsayılan olarak aktif
        };

        _unitOfWork.CompanyRepository.Add(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CompanyDto>(company);
    }
}