using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Settings.Currencies.Queries;

public class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQuery, IEnumerable<CurrencyDto>>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IMapper _mapper;

    public GetCurrenciesQueryHandler(ICurrencyRepository currencyRepository, IMapper mapper)
    {
        _currencyRepository = currencyRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CurrencyDto>> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var currencies = await _currencyRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CurrencyDto>>(currencies);
    }
}
