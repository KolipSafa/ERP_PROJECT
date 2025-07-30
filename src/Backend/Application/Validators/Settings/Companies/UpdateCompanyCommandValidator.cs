using Application.Features.Settings.Companies.Commands;
using Core.Domain.Interfaces;
using FluentValidation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Validators.Settings.Companies;

public class UpdateCompanyCommandValidator : CompanyValidatorBase<UpdateCompanyCommand>
{
    private readonly ICompanyRepository _companyRepository;

    public UpdateCompanyCommandValidator(ICompanyRepository companyRepository) : base(companyRepository)
    {
        _companyRepository = companyRepository;

        RuleFor(p => p.Id).NotEmpty();
        
        RuleFor(p => p.Name)
            .MustAsync(BeUniqueNameForUpdate).WithMessage("Bu isimde bir firma zaten mevcut.")
            .When(p => p.Name != null);
    }

    private async Task<bool> BeUniqueNameForUpdate(UpdateCompanyCommand command, string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name)) return true;
        
        var allCompanies = await _companyRepository.GetAllAsync();
        return allCompanies.All(c => !c.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase) || c.Id == command.Id);
    }
}
