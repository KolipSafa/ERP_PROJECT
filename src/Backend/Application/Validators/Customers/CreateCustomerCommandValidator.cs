using Application.Features.Customers.Commands;
using Core.Domain.Interfaces;
using FluentValidation;

namespace Application.Validators.Customers
{
    public class CreateCustomerCommandValidator : CustomerValidatorBase<CreateCustomerCommand>
    {
        public CreateCustomerCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            RuleFor(p => p.FirstName).NotNull().WithMessage("Müşteri adı zorunludur.");
            RuleFor(p => p.LastName).NotNull().WithMessage("Müşteri soyadı zorunludur.");
            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("E-posta adresi zorunludur.")
                .MustAsync(IsEmailUnique).WithMessage("Bu e-posta adresi zaten kullanılıyor.");

            ValidateFirstName();
            ValidateLastName();
            ValidateEmail();
        }
    }
}
