// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\Application\Validators\Customers\CustomerValidatorBase.cs
using Core.Domain.Interfaces;
using FluentValidation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Validators.Customers
{
    public abstract class CustomerValidatorBase<T> : AbstractValidator<T> where T : class
    {
        private readonly IUnitOfWork _unitOfWork;

        protected CustomerValidatorBase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected void ValidateFirstName()
        {
            RuleFor(x => (x as ICustomerFirstName)!.FirstName)
                .NotEmpty().WithMessage("Müşteri adı boş olamaz.")
                .MaximumLength(50).WithMessage("Müşteri adı 50 karakterden uzun olamaz.")
                .When(x => (x as ICustomerFirstName)!.FirstName != null);
        }

        protected void ValidateLastName()
        {
            RuleFor(x => (x as ICustomerLastName)!.LastName)
                .NotEmpty().WithMessage("Müşteri soyadı boş olamaz.")
                .MaximumLength(50).WithMessage("Müşteri soyadı 50 karakterden uzun olamaz.")
                .When(x => (x as ICustomerLastName)!.LastName != null);
        }

        protected void ValidateEmail()
        {
            RuleFor(x => (x as ICustomerEmail)!.Email)
                .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi girin.")
                .When(x => !string.IsNullOrEmpty((x as ICustomerEmail)!.Email));
        }
        
        // Bu metotları alt sınıflarda MustAsync ile kullanacağız.
        protected async Task<bool> IsEmailUnique(string? email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(email)) return true;
            // Not: Bu, tüm müşterileri çekip bellekte filtreler. Daha verimli bir yol,
            // repository'ye GetByEmailAsync gibi bir metot eklemektir. Şimdilik bu yeterli.
            var customers = await _unitOfWork.CustomerRepository.GetAllAsync(true);
            return !customers.Any(c => c.Email?.ToLower() == email.ToLower());
        }
    }

    // Komutların özelliklerini tip-güvenli bir şekilde belirtmek için arayüzler
    public interface ICustomerFirstName { string? FirstName { get; set; } }
    public interface ICustomerLastName { string? LastName { get; set; } }
    public interface ICustomerEmail { string? Email { get; set; } }
}
