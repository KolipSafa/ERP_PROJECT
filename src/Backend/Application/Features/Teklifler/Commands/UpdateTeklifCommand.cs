using Application.DTOs;
using Application.Common.Exceptions;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Teklifler.Commands
{
    public class UpdateTeklifCommand : IRequest<TeklifDto>
    {
        // Id'nin komut içinde olması, handler'ın hangi entity'yi güncelleyeceğini bilmesi için gereklidir.
        // Bu Id, Controller katmanında URL'den gelen Id ile doldurulacak.
        public Guid Id { get; set; }

        // PATCH işlemine uygun olarak tüm alanlar nullable (opsiyonel).
        public Guid? MusteriId { get; set; }
        public DateTime? TeklifTarihi { get; set; }
        public DateTime? GecerlilikTarihi { get; set; }
        public Core.Domain.Enums.QuoteStatus? Durum { get; set; }
        public bool? IsActive { get; set; }

        // Not: Teklif satırlarını bu komut üzerinden güncellemek karmaşık bir işlemdir.
        // Genellikle satırlar için ayrı Add/Update/Delete endpoint'leri oluşturulur.
        // Bu komut şimdilik sadece ana teklif bilgilerini güncelleyecektir.
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

            // "Akıllı Güncelleme": Sadece request'te dolu gelen alanları güncelle.
            teklifToUpdate.MusteriId = request.MusteriId ?? teklifToUpdate.MusteriId;
            teklifToUpdate.TeklifTarihi = request.TeklifTarihi ?? teklifToUpdate.TeklifTarihi;
            teklifToUpdate.GecerlilikTarihi = request.GecerlilikTarihi ?? teklifToUpdate.GecerlilikTarihi;
            teklifToUpdate.Durum = request.Durum ?? teklifToUpdate.Durum;
            teklifToUpdate.IsActive = request.IsActive ?? teklifToUpdate.IsActive;

            // Not: Toplam tutar ve satır güncellemeleri bu komutun kapsamı dışındadır.
            // Gerekirse, satırlar değiştiğinde toplam tutarı yeniden hesaplayan bir iş mantığı eklenebilir.

            _unitOfWork.TeklifRepository.Update(teklifToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<TeklifDto>(teklifToUpdate);
        }
    }
}
