using MediatR;

namespace Application.Features.Settings.Currencies.Commands;

public record DeleteCurrencyCommand(int Id) : IRequest;
