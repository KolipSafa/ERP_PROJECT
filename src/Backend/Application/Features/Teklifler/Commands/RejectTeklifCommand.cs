using Application.Common.Exceptions;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Teklifler.Commands;

public record RejectTeklifCommand(Guid Id, Guid CurrentUserId) : IRequest<Unit>;

public class RejectTeklifCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RejectTeklifCommand, Unit>
{
    public async Task<Unit> Handle(RejectTeklifCommand request, CancellationToken cancellationToken)
    {
        var teklif = await unitOfWork.TeklifRepository.GetByIdAsync(request.Id);

        if (teklif == null)
        {
            throw new NotFoundException(nameof(Teklif), request.Id);
        }

        // Güvenlik Kontrolü: İşlemi yapan kullanıcı, teklifin bağlı olduğu müşterinin ApplicationUserId'si mi?
        if (teklif.Musteri?.ApplicationUserId == null || teklif.Musteri.ApplicationUserId.Value != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException("Bu teklif üzerinde işlem yapma yetkiniz yok.");
        }

        if (teklif.Durum != QuoteStatus.Sunuldu)
        {
            throw new InvalidOperationException("Sadece 'Sunuldu' durumundaki teklifler reddedilebilir.");
        }

        teklif.Durum = QuoteStatus.Reddedildi;

        // Rezerve edilen miktarları serbest bırak
        foreach (var satir in teklif.TeklifSatirlari)
        {
            var product = await unitOfWork.ProductRepository.GetByIdAsync(satir.UrunId);
            if (product != null)
            {
                product.ReservedQuantity -= (int)satir.Miktar;
                unitOfWork.ProductRepository.Update(product);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
