using Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;

namespace Application.Features.Teklifler.Commands
{
    /// <summary>
    /// Yeni bir teklif oluşturmak için kullanılan CQRS komutu.
    /// </summary>
    public class CreateTeklifCommand : IRequest<TeklifDto>
    {
        public Guid MusteriId { get; set; }
        public DateTime TeklifTarihi { get; set; }
        public DateTime GecerlilikTarihi { get; set; }
        
        // Teklif oluşturulurken satırlar da birlikte gönderilir.
        public List<CreateTeklifSatiriDto> TeklifSatirlari { get; set; } = new();
    }

    /// <summary>
    /// Teklif oluşturulurken her bir satır için gönderilecek veriyi temsil eden DTO.
    /// </summary>
    public class CreateTeklifSatiriDto
    {
        public int UrunId { get; set; }
        public string? Aciklama { get; set; }
        public decimal Miktar { get; set; }
        public decimal BirimFiyat { get; set; } // Fiyatın o anki kopyası alınır.
    }


    /// <summary>
    /// CreateTeklifCommand'i işleyen handler.
    /// </summary>
    public class CreateTeklifCommandHandler : IRequestHandler<CreateTeklifCommand, TeklifDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateTeklifCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TeklifDto> Handle(CreateTeklifCommand request, CancellationToken cancellationToken)
        {
            // 1. Yeni Teklif ana nesnesini oluştur
            var yeniTeklif = new Teklif
            {
                Id = Guid.NewGuid(),
                MusteriId = request.MusteriId,
                TeklifTarihi = request.TeklifTarihi,
                GecerlilikTarihi = request.GecerlilikTarihi,
                Durum = QuoteStatus.Hazirlaniyor, // Yeni teklif her zaman bu durumla başlar.
                IsActive = true,
                TeklifNumarasi = await GenerateNewTeklifNumarasi() // Otomatik numara üreteceğiz.
            };

            decimal toplamTutar = 0;

            // 2. Gelen DTO'lardan TeklifSatiri entity'lerini oluştur
            foreach (var satirDto in request.TeklifSatirlari)
            {
                var toplam = satirDto.Miktar * satirDto.BirimFiyat;
                yeniTeklif.TeklifSatirlari.Add(new TeklifSatiri
                {
                    Id = Guid.NewGuid(),
                    TeklifId = yeniTeklif.Id,
                    UrunId = satirDto.UrunId,
                    Aciklama = satirDto.Aciklama ?? string.Empty,
                    Miktar = satirDto.Miktar,
                    BirimFiyat = satirDto.BirimFiyat,
                    Toplam = toplam
                });
                toplamTutar += toplam;
            }

            yeniTeklif.ToplamTutar = toplamTutar;

            // 3. Unit of Work aracılığıyla veritabanına ekle
            _unitOfWork.TeklifRepository.Add(yeniTeklif);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Sonucu DTO olarak döndür
            // Kayıt sonrası veriyi tekrar okuyarak ilişkili alanların (Müşteri Adı vb.) DTO'ya dolmasını sağlıyoruz.
            var olusturulanTeklif = await _unitOfWork.TeklifRepository.GetByIdAsync(yeniTeklif.Id);
            return _mapper.Map<TeklifDto>(olusturulanTeklif);
        }

        private Task<string> GenerateNewTeklifNumarasi()
        {
            // Bu kısım normalde veritabanından son numarayı alıp bir artırarak daha karmaşık yapılır.
            // Şimdilik basit bir örnek olarak tarih ve rastgele bir sayı kullanıyoruz.
            var today = DateTime.UtcNow;
            // Not: Gerçek bir sistemde bu yöntem çakışmalara neden olabilir.
            // Daha sağlam bir yöntem, veritabanında bir sayaç tutmaktır.
            var randomSuffix = new Random().Next(1000, 9999);
            var teklifNumarasi = $"TEK-{today:yyyyMMdd}-{randomSuffix}";
            return Task.FromResult(teklifNumarasi);
        }
    }
}
