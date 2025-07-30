using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Settings.Companies.Commands;

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyToUpdate = await _unitOfWork.CompanyRepository.GetByIdAsync(request.Id);
        if (companyToUpdate == null)
        {
            throw new ValidationException($"ID'si {request.Id} olan firma bulunamadı.");
        }

        // Smart update
        if (request.Name != null)
        {
            companyToUpdate.Name = request.Name;
        }
        if (request.TaxNumber != null)
        {
            companyToUpdate.TaxNumber = request.TaxNumber;
        }
        if (request.Address != null)
        {
            companyToUpdate.Address = request.Address;
        }
        if (request.IsActive.HasValue)
        {
            companyToUpdate.IsActive = request.IsActive.Value;
        }

        _unitOfWork.CompanyRepository.Update(companyToUpdate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<CompanyDto>(companyToUpdate);
    }
}