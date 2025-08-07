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

            // --- Rezerve Miktarlarını Güncelleme Mantığı ---
            // 1. Önceki tüm rezerve miktarlarını geri al
            foreach (var mevcutSatir in teklifToUpdate.TeklifSatirlari)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(mevcutSatir.UrunId);
                if (product != null)
                {
                    product.ReservedQuantity -= (int)mevcutSatir.Miktar;
                    _unitOfWork.ProductRepository.Update(product);
                }
            }

            // Ana teklif bilgilerini güncelle
            teklifToUpdate.MusteriId = request.MusteriId ?? teklifToUpdate.MusteriId;
            teklifToUpdate.TeklifTarihi = request.TeklifTarihi ?? teklifToUpdate.TeklifTarihi;
            teklifToUpdate.GecerlilikTarihi = request.GecerlilikTarihi ?? teklifToUpdate.GecerlilikTarihi;
            teklifToUpdate.CurrencyId = request.CurrencyId ?? teklifToUpdate.CurrencyId;
            teklifToUpdate.Durum = request.Durum ?? teklifToUpdate.Durum;
            teklifToUpdate.IsActive = request.IsActive ?? teklifToUpdate.IsActive;

            // --- Satırları Yönetme Mantığı ---
            var gelenSatirIdleri = request.TeklifSatirlari.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToList();
            
            // 2. Silinmiş Satırları Bul ve Kaldır
            var silinecekSatirlar = teklifToUpdate.TeklifSatirlari.Where(s => !gelenSatirIdleri.Contains(s.Id)).ToList();
            foreach (var satir in silinecekSatirlar)
            {
                _unitOfWork.TeklifRepository.DeleteSatir(satir);
            }

            // 3. Mevcut ve Yeni Satırları Güncelle/Ekle
            foreach (var satirDto in request.TeklifSatirlari)
            {
                if (satirDto.Id.HasValue && !satirDto.Id.ToString().StartsWith("new_"))
                {
                    // Mevcut satırı güncelle
                    var mevcutSatir = teklifToUpdate.TeklifSatirlari.FirstOrDefault(s => s.Id == satirDto.Id.Value);
                    if (mevcutSatir != null)
                    {
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
                        TeklifId = teklifToUpdate.Id,
                        UrunId = satirDto.UrunId,
                        Aciklama = satirDto.Aciklama,
                        Miktar = satirDto.Miktar,
                        BirimFiyat = satirDto.BirimFiyat,
                        Toplam = satirDto.Miktar * satirDto.BirimFiyat
                    };
                    teklifToUpdate.TeklifSatirlari.Add(yeniSatir);
                }
            }

            // 4. Yeni rezerve miktarlarını ayarla ve toplam tutarı hesapla
            teklifToUpdate.ToplamTutar = 0;
            foreach (var satir in teklifToUpdate.TeklifSatirlari)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(satir.UrunId);
                if (product != null)
                {
                    product.ReservedQuantity += (int)satir.Miktar;
                    _unitOfWork.ProductRepository.Update(product);
                }
                teklifToUpdate.ToplamTutar += satir.Toplam;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var guncelTeklif = await _unitOfWork.TeklifRepository.GetByIdAsync(request.Id);
            return _mapper.Map<TeklifDto>(guncelTeklif!);
        }
    }
}