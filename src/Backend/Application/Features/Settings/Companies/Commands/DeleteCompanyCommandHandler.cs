using Application.Common.Exceptions;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Settings.Companies.Commands
{
    public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCompanyCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _unitOfWork.CompanyRepository.GetByIdAsync(request.Id);

            if (company == null)
            {
                throw new NotFoundException(nameof(Company), request.Id);
            }

            // Soft delete mantığı Repository katmanında uygulanmıştı.
            _unitOfWork.CompanyRepository.Delete(company);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
