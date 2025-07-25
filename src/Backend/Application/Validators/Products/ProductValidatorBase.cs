using Core.Domain.Interfaces;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Validators.Products
{
    public abstract class ProductValidatorBase<T> : AbstractValidator<T> where T : class
    {
        private readonly IUnitOfWork _unitOfWork;

        protected ProductValidatorBase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected void ValidateName()
        {
            RuleFor(x => (x as IProductName)!.Name)
                .NotEmpty().WithMessage("Ürün adı boş olamaz.")
                .MaximumLength(100).WithMessage("Ürün adı 100 karakterden uzun olamaz.")
                .When(x => (x as IProductName)!.Name != null);
        }

        protected void ValidatePrice()
        {
            RuleFor(x => (x as IProductPrice)!.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.")
                .When(x => (x as IProductPrice)!.Price.HasValue);
        }

        protected void ValidateStock()
        {
            RuleFor(x => (x as IProductStock)!.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz.")
                .When(x => (x as IProductStock)!.StockQuantity.HasValue);
        }

        protected async Task<bool> BeUniqueSku(string sku, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(sku)) return true;
            return await _unitOfWork.ProductRepository.GetBySkuAsync(sku) == null;
        }
    }

    public interface IProductName { string? Name { get; } }
    public interface IProductPrice { decimal? Price { get; } }
    public interface IProductStock { int? StockQuantity { get; } }
    public interface IProductSku { string? SKU { get; } }
}
