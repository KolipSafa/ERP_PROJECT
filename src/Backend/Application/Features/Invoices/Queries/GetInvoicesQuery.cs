using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Invoices.Queries
{
    public record GetInvoicesQuery(Guid? CustomerId, ClaimsPrincipal User) : IRequest<IEnumerable<InvoiceDto>>;

    public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, IEnumerable<InvoiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetInvoicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
        {
            Guid? customerId = request.CustomerId;

            // Customer rolüyse, ClaimsPrincipal'dan ApplicationUserId al ve Customer tablosundan eşleştir
            if (request.User.IsInRole("Customer") && !customerId.HasValue)
            {
                var sub = request.User.FindFirstValue("sub")
                          ?? request.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(sub, out var appUserId))
                {
                    // ApplicationUserId -> Customer.Id eşlemesi
                    var customers = await _unitOfWork.CustomerRepository.GetAllAsync();
                    var me = customers.FirstOrDefault(c => c.ApplicationUserId == appUserId);
                    if (me != null) customerId = me.Id;
                }
            }

            if (!customerId.HasValue && request.User.IsInRole("Customer"))
            {
                // Müşteri ama customerId bulunamadı → boş döndür
                return Enumerable.Empty<InvoiceDto>();
            }

            IEnumerable<Core.Domain.Entities.Invoice> invoices;
            if (customerId.HasValue)
            {
                invoices = await _unitOfWork.InvoiceRepository.GetAllByCustomerIdAsync(customerId.Value);
            }
            else
            {
                // Admin için tüm faturalar
                invoices = await _unitOfWork.InvoiceRepository.GetAllAsync();
            }

            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }
    }
}


