using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Settings.Currencies.Commands;

public class CreateCurrencyCommandHandler : IRequestHandler<CreateCurrencyCommand, CurrencyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCurrencyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CurrencyDto> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = new Currency
        {
            Name = request.Name,
            Code = request.Code.ToUpper(),
            Symbol = request.Symbol,
            IsActive = true // Yeni eklenenler varsayılan olarak aktif
        };

        _unitOfWork.CurrencyRepository.Add(currency);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CurrencyDto>(currency);
    }
}
