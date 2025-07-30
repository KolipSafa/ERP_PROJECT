using Application.Features.Settings.Currencies.Commands;
using Core.Domain.Interfaces;
using FluentValidation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Validators.Settings.Currencies;

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    private readonly ICurrencyRepository _currencyRepository;

    public UpdateCurrencyCommandValidator(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;

        RuleFor(v => v.Id).NotEmpty();

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Para birimi adı boş olamaz.")
            .MaximumLength(50).WithMessage("Para birimi adı 50 karakterden uzun olamaz.");

        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Para birimi kodu boş olamaz.")
            .Length(3).WithMessage("Para birimi kodu 3 karakter olmalıdır.")
            .MustAsync(BeUniqueCodeForUpdate).WithMessage("Bu kod ile bir para birimi zaten mevcut.");

        RuleFor(v => v.Symbol)
            .NotEmpty().WithMessage("Sembol boş olamaz.")
            .MaximumLength(5).WithMessage("Sembol 5 karakterden uzun olamaz.");
    }

    private async Task<bool> BeUniqueCodeForUpdate(UpdateCurrencyCommand command, string code, CancellationToken cancellationToken)
    {
        var allCurrencies = await _currencyRepository.GetAllAsync();
        return allCurrencies.All(c => !c.Code.Equals(code, System.StringComparison.OrdinalIgnoreCase) || c.Id == command.Id);
    }
}
