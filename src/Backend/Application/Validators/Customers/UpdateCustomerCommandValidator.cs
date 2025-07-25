using Application.Features.Customers.Commands;
using Core.Domain.Interfaces;
using FluentValidation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Validators.Customers
{
    public class UpdateCustomerCommandValidator : CustomerValidatorBase<UpdateCustomerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCustomerCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            RuleFor(p => p.Id).NotEmpty();
            RuleFor(p => p.Email)
                .MustAsync(IsEmailUniqueForUpdate).WithMessage("Bu e-posta adresi zaten başka bir müşteriye ait.");

            ValidateFirstName();
            ValidateLastName();
            ValidateEmail();
        }

        private async Task<bool> IsEmailUniqueForUpdate(UpdateCustomerCommand command, string? email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(email)) return true;
            var customers = await _unitOfWork.CustomerRepository.GetAllAsync(true);
            return !customers.Any(c => c.Email?.ToLower() == email.ToLower() && c.Id != command.Id);
        }
    }
}
