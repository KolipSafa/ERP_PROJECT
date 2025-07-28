using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Teklifler.Queries
{
    /// <summary>
    /// Belirtilen ID'ye sahip tek bir teklifin tüm detaylarını getirmek için kullanılan CQRS sorgusu.
    /// </summary>
    public class GetTeklifByIdQuery : IRequest<TeklifDto?>
    {
        public Guid Id { get; set; }

        public GetTeklifByIdQuery(Guid id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// GetTeklifByIdQuery sorgusunu işleyen handler.
    /// </summary>
    public class GetTeklifByIdQueryHandler : IRequestHandler<GetTeklifByIdQuery, TeklifDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTeklifByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TeklifDto?> Handle(GetTeklifByIdQuery request, CancellationToken cancellationToken)
        {
            var teklif = await _unitOfWork.TeklifRepository.GetByIdAsync(request.Id);

            if (teklif == null)
            {
                return null; // Teklif bulunamazsa null dön. Controller bunu 404 Not Found olarak yorumlayacak.
            }

            // AutoMapper, bizim için Teklif entity'sini, satırları ve diğer tüm ilişkili
            // bilgileriyle birlikte eksiksiz bir TeklifDto'ya dönüştürecek.
            return _mapper.Map<TeklifDto>(teklif);
        }
    }
}
