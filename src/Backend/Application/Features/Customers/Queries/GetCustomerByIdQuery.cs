using Application.DTOs;
using Application.Interfaces;
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
        private readonly ISupabaseAuthAdminService _supabaseService;

        public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ISupabaseAuthAdminService supabaseService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _supabaseService = supabaseService;
        }

        public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(request.Id);

            if (customer == null)
            {
                return null;
            }

            var customerDto = _mapper.Map<CustomerDto>(customer);

            if (customer.ApplicationUserId.HasValue)
            {
                var supabaseUser = await _supabaseService.GetUserById(customer.ApplicationUserId.Value.ToString(), cancellationToken);
                var isEmailConfirmed = supabaseUser?.EmailConfirmedAt.HasValue ?? false;
                var isAppMetaActive = false;
                try
                {
                    if (supabaseUser?.AppMetadata != null && supabaseUser.AppMetadata.TryGetValue("status", out var statusObj))
                    {
                        var statusStr = statusObj?.ToString();
                        isAppMetaActive = string.Equals(statusStr, "active", StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch { /* ignore parsing issues */ }
                customerDto.IsAccountActive = isEmailConfirmed || isAppMetaActive;
            }
            else
            {
                customerDto.IsAccountActive = false;
            }

            return customerDto;
        }
    }
}
