using Application.Common.Exceptions;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Settings.Companies.Commands
{
    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _unitOfWork.CompanyRepository.GetByIdAsync(request.Id);

            if (company == null)
            {
                throw new NotFoundException(nameof(Company), request.Id);
            }

            _mapper.Map(request, company);

            _unitOfWork.CompanyRepository.Update(company);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
