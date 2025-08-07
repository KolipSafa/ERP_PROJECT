using Application.Common.Exceptions;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Teklifler.Commands
{
    public record ApproveTeklifCommand(Guid TeklifId, Guid CurrentUserId) : IRequest;

    public class ApproveTeklifCommandHandler : IRequestHandler<ApproveTeklifCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApproveTeklifCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ApproveTeklifCommand request, CancellationToken cancellationToken)
        {
            var teklif = await _unitOfWork.TeklifRepository.GetByIdAsync(request.TeklifId);

            if (teklif == null)
            {
                throw new NotFoundException(nameof(Teklif), request.TeklifId);
            }

            // Güvenlik Kontrolü: İşlemi yapan kullanıcı, teklifin sahibi mi?
            if (teklif.MusteriId != request.CurrentUserId)
            {
                // Bu hata, yetkisiz bir kullanıcının başka birinin teklifini onaylamasını engeller.
                throw new UnauthorizedAccessException("Bu teklif üzerinde işlem yapma yetkiniz yok.");
            }

            if (teklif.Durum != QuoteStatus.Sunuldu)
            {
                throw new InvalidOperationException("Sadece 'Sunuldu' durumundaki teklifler onaylanabilir.");
            }

            // 1. Teklifin durumunu güncelle
            teklif.Durum = QuoteStatus.Onaylandı;

            // 2. Rezerve edilen miktarları serbest bırak
            foreach (var satir in teklif.TeklifSatirlari)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(satir.UrunId);
                if (product != null)
                {
                    product.ReservedQuantity -= (int)satir.Miktar;
                    _unitOfWork.ProductRepository.Update(product);
                }
            }

            // 3. Fatura oluştur
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                CustomerId = teklif.MusteriId,
                TeklifId = teklif.Id,
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30), // Örnek: 30 gün vade
                TotalAmount = teklif.ToplamTutar,
                Status = InvoiceStatus.Sent, // Planımıza göre 'Gönderildi' olarak başlatıyoruz
                InvoiceNumber = GenerateNewInvoiceNumber()
            };

            foreach (var satir in teklif.TeklifSatirlari)
            {
                invoice.InvoiceLines.Add(new InvoiceLine
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ProductId = satir.UrunId,
                    Description = satir.Aciklama,
                    Quantity = satir.Miktar,
                    UnitPrice = satir.BirimFiyat,
                    Total = satir.Toplam
                });
            }

            _unitOfWork.InvoiceRepository.Add(invoice);

            // 4. Tüm değişiklikleri tek bir transaction'da kaydet
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private string GenerateNewInvoiceNumber()
        {
            // Gerçek bir sistemde, bu daha karmaşık ve çakışmaya dayanıklı olmalıdır.
            var today = DateTime.UtcNow;
            var randomSuffix = new Random().Next(1000, 9999);
            return $"FAT-{today:yyyyMMdd}-{randomSuffix}";
        }
    }
}
