using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Settings.Currencies.Commands;

public class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, CurrencyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCurrencyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CurrencyDto> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currencyToUpdate = await _unitOfWork.CurrencyRepository.GetByIdAsync(request.Id);
        if (currencyToUpdate == null)
        {
            throw new ValidationException($"ID'si {request.Id} olan para birimi bulunamadı.");
        }

        currencyToUpdate.Name = request.Name;
        currencyToUpdate.Code = request.Code.ToUpper();
        currencyToUpdate.Symbol = request.Symbol;
        currencyToUpdate.IsActive = request.IsActive;

        _unitOfWork.CurrencyRepository.Update(currencyToUpdate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<CurrencyDto>(currencyToUpdate);
    }
}
