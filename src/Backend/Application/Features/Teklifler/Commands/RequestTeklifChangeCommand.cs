using Application.Common.Exceptions;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Application.Features.Teklifler.Commands
{
    // Müşterinin değişiklik taleplerini iletmek için DTO'lar
    public class TeklifChangeRequestDto
    {
        public string? Notes { get; set; }
        public List<TeklifSatiriChangeDto> UpdatedLines { get; set; } = new();
    }

    public class TeklifSatiriChangeDto
    {
        public Guid Id { get; set; }
        public decimal NewQuantity { get; set; }
    }

    public record RequestTeklifChangeCommand(Guid TeklifId, Guid CurrentUserId, TeklifChangeRequestDto ChangeRequest) : IRequest;

    public class RequestTeklifChangeCommandHandler : IRequestHandler<RequestTeklifChangeCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RequestTeklifChangeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(RequestTeklifChangeCommand request, CancellationToken cancellationToken)
        {
            var teklif = await _unitOfWork.TeklifRepository.GetByIdAsync(request.TeklifId);

            if (teklif == null)
            {
                throw new NotFoundException(nameof(Teklif), request.TeklifId);
            }

            // Güvenlik Kontrolü: İşlemi yapan kullanıcı, teklifin bağlı olduğu müşterinin ApplicationUserId'si mi?
            if (teklif.Musteri?.ApplicationUserId == null || teklif.Musteri.ApplicationUserId.Value != request.CurrentUserId)
            {
                throw new UnauthorizedAccessException("Bu teklif üzerinde işlem yapma yetkiniz yok.");
            }

            if (teklif.Durum != QuoteStatus.Sunuldu)
            {
                throw new InvalidOperationException("Sadece 'Sunuldu' durumundaki teklifler için değişiklik talep edilebilir.");
            }

            // Teklifin durumunu güncelle
            teklif.Durum = QuoteStatus.ChangeRequested;

            // TODO: Müşterinin notlarını veya satır değişiklik taleplerini
            // veritabanında ayrı bir yere kaydetmek (örn: TeklifRevizyonları tablosu).
            // Şimdilik sadece durumu güncelliyoruz. Bu, ileride geliştirilebilir.

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
