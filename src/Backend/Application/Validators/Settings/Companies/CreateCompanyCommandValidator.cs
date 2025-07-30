using Application.Features.Settings.Companies.Commands;
using Core.Domain.Interfaces;
using FluentValidation;

namespace Application.Validators.Settings.Companies;

public class CreateCompanyCommandValidator : CompanyValidatorBase<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator(ICompanyRepository companyRepository) : base(companyRepository)
    {
        RuleFor(v => v.Name)
            .MustAsync(BeUniqueName).WithMessage("Bu isimde bir firma zaten mevcut.");

        RuleFor(v => v.TaxNumber)
            .MaximumLength(20).WithMessage("Vergi numarası 20 karakterden uzun olamaz.");

        RuleFor(v => v.Address)
            .MaximumLength(500).WithMessage("Adres 500 karakterden uzun olamaz.");
    }
}
