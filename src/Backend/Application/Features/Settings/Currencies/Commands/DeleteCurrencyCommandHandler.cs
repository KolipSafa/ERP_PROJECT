using Core.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Settings.Currencies.Commands;

public class DeleteCurrencyCommandHandler : IRequestHandler<DeleteCurrencyCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCurrencyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currencyToDelete = await _unitOfWork.CurrencyRepository.GetByIdAsync(request.Id);
        if (currencyToDelete == null)
        {
            throw new ValidationException($"ID'si {request.Id} olan para birimi bulunamadı.");
        }

        _unitOfWork.CurrencyRepository.Delete(currencyToDelete);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
