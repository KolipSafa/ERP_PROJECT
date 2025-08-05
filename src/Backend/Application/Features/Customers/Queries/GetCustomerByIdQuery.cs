using Application.DTOs;
using AutoMapper;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Queries
{
    public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;

    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(request.Id);

            if (customer == null)
            {
                return null;
            }

            var customerDto = _mapper.Map<CustomerDto>(customer);

            // TODO: Müşterinin Supabase'deki hesap durumunu (EmailConfirmed)
            // Supabase Admin API'si üzerinden sorgulayarak `IsAccountActive` alanını doldur.
            customerDto.IsAccountActive = false; // Şimdilik varsayılan olarak false ayarlıyoruz.

            return customerDto;
        }
    }
}
