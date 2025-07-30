using Application.Features.Products.Commands;
using Core.Domain.Interfaces;
using FluentValidation;

namespace Application.Validators.Products
{
    public class CreateProductCommandValidator : ProductValidatorBase<CreateProductCommand>
    {
        public CreateProductCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            // Bu alanların Create işlemi için zorunlu olduğunu belirtiyoruz.
            RuleFor(p => p.Name).NotNull().WithMessage("Ürün adı zorunludur.");
            RuleFor(p => p.Price).NotNull().WithMessage("Fiyat zorunludur.");
            RuleFor(p => p.StockQuantity).NotNull().WithMessage("Stok miktarı zorunludur.");
            RuleFor(p => p.CurrencyId).NotNull().WithMessage("Para birimi seçimi zorunludur.");
            
            // Yeni kural: Açıklama boş olamaz.
            RuleFor(p => p.Description).NotEmpty().WithMessage("Açıklama alanı zorunludur.");

            // Temel sınıftaki metotları çağırarak ortak kuralları uyguluyoruz.
            ValidateName();
            ValidatePrice();
            ValidateStock(); // Stok >= 0 kuralını uygular.
        }
    }
}
