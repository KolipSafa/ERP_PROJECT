using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Settings.Companies.Commands
{
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
            var company = _mapper.Map<Company>(request);
            company.IsActive = true; // Yeni firmalar varsayılan olarak aktif

            _unitOfWork.CompanyRepository.Add(company);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CompanyDto>(company);
        }
    }
}
