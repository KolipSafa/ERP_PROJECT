using Application.DTOs;
using AutoMapper;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Teklifler.Queries
{
    /// <summary>
    /// Tüm teklifleri listelemek için kullanılan, gelişmiş filtreleme ve sıralama yeteneklerine sahip CQRS sorgusu.
    /// </summary>
    public class GetAllTekliflerQuery : IRequest<IEnumerable<TeklifDto>>
    {
        public Guid? MusteriId { get; set; }
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public QuoteStatus? Durum { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } // "asc" veya "desc"
    }

    /// <summary>
    /// GetAllTekliflerQuery sorgusunu işleyen handler.
    /// </summary>
    public class GetAllTekliflerQueryHandler : IRequestHandler<GetAllTekliflerQuery, IEnumerable<TeklifDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllTekliflerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TeklifDto>> Handle(GetAllTekliflerQuery request, CancellationToken cancellationToken)
        {
            var teklifler = await _unitOfWork.TeklifRepository.GetAllAsync(
                request.MusteriId,
                request.BaslangicTarihi,
                request.BitisTarihi,
                request.Durum,
                request.IncludeInactive,
                request.SortBy,
                request.SortOrder
            );
            
            return _mapper.Map<IEnumerable<TeklifDto>>(teklifler);
        }
    }
}
