using Application.Features.Settings.Currencies.Commands;
using Core.Domain.Interfaces;
using FluentValidation;

namespace Application.Validators.Settings.Currencies;

public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    private readonly ICurrencyRepository _currencyRepository;

    public CreateCurrencyCommandValidator(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Para birimi adı boş olamaz.")
            .MaximumLength(50).WithMessage("Para birimi adı 50 karakterden uzun olamaz.");

        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Para birimi kodu boş olamaz.")
            .Length(3).WithMessage("Para birimi kodu 3 karakter olmalıdır.")
            .MustAsync(BeUniqueCode).WithMessage("Bu kod ile bir para birimi zaten mevcut.");

        RuleFor(v => v.Symbol)
            .NotEmpty().WithMessage("Sembol boş olamaz.")
            .MaximumLength(5).WithMessage("Sembol 5 karakterden uzun olamaz.");
    }

    private async Task<bool> BeUniqueCode(string code, CancellationToken cancellationToken)
    {
        var allCurrencies = await _currencyRepository.GetAllAsync();
        return allCurrencies.All(c => c.Code.ToUpper() != code.ToUpper());
    }
}
