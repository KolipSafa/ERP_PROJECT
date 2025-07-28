using Application.Features.Teklifler.Commands;
using Core.Domain.Interfaces;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Validators.Teklifler
{
    public class UpdateTeklifCommandValidator : AbstractValidator<UpdateTeklifCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTeklifCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.Id).NotEmpty().WithMessage("Güncellenecek teklifin ID'si boş olamaz.");

            // Sadece MusteriId dolu olarak geldiyse kontrol et.
            RuleFor(v => v.MusteriId)
                .MustAsync(MusteriVarMi)
                .WithMessage("Belirtilen müşteri bulunamadı.")
                .When(v => v.MusteriId.HasValue);

            // Sadece tarihler doluysa ve birlikte doluysa kontrol et.
            When(v => v.TeklifTarihi.HasValue && v.GecerlilikTarihi.HasValue, () =>
            {
                RuleFor(v => v.GecerlilikTarihi)
                    .GreaterThan(v => v.TeklifTarihi)
                    .WithMessage("Geçerlilik tarihi, teklif tarihinden sonra olmalıdır.");
            });
        }

        private async Task<bool> MusteriVarMi(Guid? musteriId, CancellationToken cancellationToken)
        {
            if (!musteriId.HasValue) return true; // Eğer ID gelmediyse, bu kuralı atla.
            var musteri = await _unitOfWork.CustomerRepository.GetByIdAsync(musteriId.Value);
            return musteri != null;
        }
    }
}
