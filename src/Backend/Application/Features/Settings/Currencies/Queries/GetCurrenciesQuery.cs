using Application.DTOs;
using MediatR;

namespace Application.Features.Settings.Currencies.Queries;

public record GetCurrenciesQuery : IRequest<IEnumerable<CurrencyDto>>;
