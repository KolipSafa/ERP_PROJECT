using Core.Domain.Interfaces;
using FluentValidation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Validators.Settings.Companies
{
    public abstract class CompanyValidatorBase<T> : AbstractValidator<T> where T : class, ICompanyName
    {
        private readonly ICompanyRepository _companyRepository;

        protected CompanyValidatorBase(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Firma adı boş olamaz.")
                .MaximumLength(200).WithMessage("Firma adı 200 karakterden uzun olamaz.")
                .When(x => x.Name != null);
        }

        protected async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(name)) return true;
            var allCompanies = await _companyRepository.GetAllAsync();
            return allCompanies.All(c => !c.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }
    }

    public interface ICompanyName 
    { 
        string? Name { get; } 
    }
}
