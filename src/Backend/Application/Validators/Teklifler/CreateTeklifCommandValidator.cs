using Application.Features.Teklifler.Commands;
using Core.Domain.Interfaces;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Validators.Teklifler
{
    public class CreateTeklifCommandValidator : AbstractValidator<CreateTeklifCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateTeklifCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.MusteriId)
                .NotEmpty().WithMessage("Müşteri ID'si boş olamaz.")
                .MustAsync(MusteriVarMi).WithMessage("Belirtilen müşteri bulunamadı.");

            RuleFor(v => v.TeklifTarihi)
                .NotEmpty().WithMessage("Teklif tarihi zorunludur.");

            RuleFor(v => v.GecerlilikTarihi)
                .NotEmpty().WithMessage("Geçerlilik tarihi zorunludur.")
                .GreaterThan(v => v.TeklifTarihi).WithMessage("Geçerlilik tarihi, teklif tarihinden sonra olmalıdır.");

            RuleFor(v => v.TeklifSatirlari)
                .NotEmpty().WithMessage("Teklif en az bir satır içermelidir.");

            // Koleksiyon içindeki her bir eleman için kural tanımlama
            RuleForEach(v => v.TeklifSatirlari).SetValidator(new CreateTeklifSatiriDtoValidator(unitOfWork));
        }

        private async Task<bool> MusteriVarMi(System.Guid musteriId, CancellationToken cancellationToken)
        {
            var musteri = await _unitOfWork.CustomerRepository.GetByIdAsync(musteriId);
            return musteri != null;
        }
    }

    public class CreateTeklifSatiriDtoValidator : AbstractValidator<CreateTeklifSatiriDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateTeklifSatiriDtoValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(s => s.UrunId)
                .NotEmpty().WithMessage("Ürün ID'si boş olamaz.")
                .MustAsync(UrunVarMi).WithMessage("Belirtilen ürün bulunamadı.");

            RuleFor(s => s.Miktar)
                .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");

            RuleFor(s => s.BirimFiyat)
                .GreaterThanOrEqualTo(0).WithMessage("Birim fiyat negatif olamaz.");
        }

        private async Task<bool> UrunVarMi(int urunId, CancellationToken cancellationToken)
        {
            var urun = await _unitOfWork.ProductRepository.GetByIdAsync(urunId);
            return urun != null;
        }
    }
}
