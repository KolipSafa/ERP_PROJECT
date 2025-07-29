using Application.DTOs;
using MediatR;

namespace Application.Features.Settings.Currencies.Commands;

public record CreateCurrencyCommand(
    string Name,
    string Code,
    string Symbol) : IRequest<CurrencyDto>;
