using Core.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Settings.Companies.Commands;

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCompanyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyToDelete = await _unitOfWork.CompanyRepository.GetByIdAsync(request.Id);
        if (companyToDelete == null)
        {
            // Proje planına göre, bulunamayan kaynaklar için NotFoundException fırlatmak daha doğru olabilir.
            // Şimdilik ValidationException ile devam ediyorum, ancak bu refactor edilebilir.
            throw new ValidationException($"ID'si {request.Id} olan firma bulunamadı.");
        }

        _unitOfWork.CompanyRepository.Delete(companyToDelete); // Bu soft delete yapacak
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}