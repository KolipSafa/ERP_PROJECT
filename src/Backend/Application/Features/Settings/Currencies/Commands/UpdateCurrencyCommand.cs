using Application.DTOs;
using MediatR;

namespace Application.Features.Settings.Currencies.Commands;

public record UpdateCurrencyCommand(
    int Id,
    string Name,
    string Code,
    string Symbol,
    bool IsActive) : IRequest<CurrencyDto>;
