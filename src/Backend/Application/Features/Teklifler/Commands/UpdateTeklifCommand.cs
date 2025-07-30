using Application.DTOs;
using Application.Common.Exceptions;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Teklifler.Commands
{
    public class UpdateTeklifCommand : IRequest<TeklifDto>
    {
        public Guid Id { get; set; }
        public Guid? MusteriId { get; set; }
        public DateTime? TeklifTarihi { get; set; }
        public DateTime? GecerlilikTarihi { get; set; }
        public int? CurrencyId { get; set; }
        public Core.Domain.Enums.QuoteStatus? Durum { get; set; }
        public bool? IsActive { get; set; }

        // Frontend'den teklifin son halindeki tüm satırları alacağız.
        public List<UpdateTeklifSatiriDto> TeklifSatirlari { get; set; } = new();
    }

    public class UpdateTeklifSatiriDto
    {
        public Guid? Id { get; set; } // Yeni satırlar için Id null olacak.
        public int UrunId { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public decimal Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
    }

    public class UpdateTeklifCommandHandler : IRequestHandler<UpdateTeklifCommand, TeklifDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateTeklifCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TeklifDto> Handle(UpdateTeklifCommand request, CancellationToken cancellationToken)
        {
            var teklifToUpdate = await _unitOfWork.TeklifRepository.GetByIdAsync(request.Id);

            if (teklifToUpdate == null)
            {
                throw new NotFoundException(nameof(Teklif), request.Id);
            }

            // Ana teklif bilgilerini AutoMapper olmadan, manuel olarak güncelle
            teklifToUpdate.MusteriId = request.MusteriId ?? teklifToUpdate.MusteriId;
            teklifToUpdate.TeklifTarihi = request.TeklifTarihi ?? teklifToUpdate.TeklifTarihi;
            teklifToUpdate.GecerlilikTarihi = request.GecerlilikTarihi ?? teklifToUpdate.GecerlilikTarihi;
            teklifToUpdate.CurrencyId = request.CurrencyId ?? teklifToUpdate.CurrencyId;
            teklifToUpdate.Durum = request.Durum ?? teklifToUpdate.Durum;
            teklifToUpdate.IsActive = request.IsActive ?? teklifToUpdate.IsActive;

            // --- Satırları Yönetme Mantığı ---
            var gelenSatirIdleri = request.TeklifSatirlari.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToList();
            
            // 1. Silinmiş Satırları Bul ve Kaldır
            var silinecekSatirlar = teklifToUpdate.TeklifSatirlari.Where(s => !gelenSatirIdleri.Contains(s.Id)).ToList();
            foreach (var satir in silinecekSatirlar)
            {
                _unitOfWork.TeklifRepository.DeleteSatir(satir);
            }

            // 2. Mevcut ve Yeni Satırları Güncelle/Ekle
            foreach (var satirDto in request.TeklifSatirlari)
            {
                if (satirDto.Id.HasValue)
                {
                    // Mevcut satırı güncelle
                    var mevcutSatir = teklifToUpdate.TeklifSatirlari.FirstOrDefault(s => s.Id == satirDto.Id.Value);
                    if (mevcutSatir != null)
                    {
                        // EF'nin bu değişikliği algılaması için DTO'dan entity'e manuel mapping yapıyoruz.
                        mevcutSatir.Aciklama = satirDto.Aciklama;
                        mevcutSatir.Miktar = satirDto.Miktar;
                        mevcutSatir.BirimFiyat = satirDto.BirimFiyat;
                        mevcutSatir.Toplam = satirDto.Miktar * satirDto.BirimFiyat;
                    }
                }
                else
                {
                    // Yeni satır ekle
                    var yeniSatir = new TeklifSatiri
                    {
                        // Id'yi burada oluşturmuyoruz, veritabanı kendisi atayacak.
                        TeklifId = teklifToUpdate.Id,
                        UrunId = satirDto.UrunId,
                        Aciklama = satirDto.Aciklama,
                        Miktar = satirDto.Miktar,
                        BirimFiyat = satirDto.BirimFiyat,
                        Toplam = satirDto.Miktar * satirDto.BirimFiyat
                    };
                    // Doğrudan teklifin koleksiyonuna eklemek EF için en temiz yoldur.
                    teklifToUpdate.TeklifSatirlari.Add(yeniSatir);
                }
            }

            // Toplam tutarı yeniden hesapla
            teklifToUpdate.ToplamTutar = teklifToUpdate.TeklifSatirlari.Sum(s => s.Toplam);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var guncelTeklif = await _unitOfWork.TeklifRepository.GetByIdAsync(request.Id);
            return _mapper.Map<TeklifDto>(guncelTeklif!);
        }
    }
}