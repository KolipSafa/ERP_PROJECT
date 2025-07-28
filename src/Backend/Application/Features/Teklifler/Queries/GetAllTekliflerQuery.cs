using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Teklifler.Queries
{
    /// <summary>
    /// Tüm teklifleri listelemek için kullanılan CQRS sorgusu.
    /// </summary>
    public class GetAllTekliflerQuery : IRequest<IEnumerable<TeklifDto>>
    {
        // Gelecekte bu alana filtreleme, sıralama ve sayfalama parametreleri eklenebilir.
        // Örneğin: public string? MusteriAdiFiltresi { get; set; }
    }

    /// <summary>
    /// GetAllTekliflerQuery sorgusunu işleyen handler.
    /// Proje standardı gereği sorgu ile aynı dosyada bulunur.
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
            var teklifler = await _unitOfWork.TeklifRepository.GetAllAsync();
            
            // AutoMapper, bizim için Teklif entity listesini, ilişkili Müşteri adını da
            // içerecek şekilde TeklifDto listesine dönüştürecek.
            return _mapper.Map<IEnumerable<TeklifDto>>(teklifler);
        }
    }
}
