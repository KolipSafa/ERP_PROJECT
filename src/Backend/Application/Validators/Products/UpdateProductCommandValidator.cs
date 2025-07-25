using Application.Features.Products.Commands;
using Core.Domain.Interfaces;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Validators.Products
{
    public class UpdateProductCommandValidator : ProductValidatorBase<UpdateProductCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(p => p.Id).NotEmpty();
            ValidateName();
            ValidatePrice();
            ValidateStock();

            RuleFor(p => p.SKU)
                .NotEmpty().WithMessage("SKU boş olamaz.")
                .MustAsync(BeUniqueSkuForUpdate).WithMessage("Bu SKU zaten başka bir ürüne ait.")
                .When(p => p.SKU != null);
        }

        private async Task<bool> BeUniqueSkuForUpdate(UpdateProductCommand command, string? sku, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(sku)) return true;
            
            var existingProduct = await _unitOfWork.ProductRepository.GetBySkuAsync(sku);
            return existingProduct == null || existingProduct.Id == command.Id;
        }
    }
}
