using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Queries
{
    public record GetAllCustomersQuery(
        bool IncludeInactive,
        string? SearchTerm,
        string? SortBy,
        bool IsDescending) : IRequest<IEnumerable<CustomerDto>>;

    public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISupabaseAuthAdminService _supabaseService;

        public GetAllCustomersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ISupabaseAuthAdminService supabaseService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _supabaseService = supabaseService;
        }

        public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _unitOfWork.CustomerRepository.GetAllAsync(
                request.IncludeInactive,
                request.SearchTerm,
                request.SortBy,
                request.IsDescending);

            var customerDtos = _mapper.Map<List<CustomerDto>>(customers);

            // TODO: Bu N+1 sorgu problemine neden olabilir. Çok sayıda müşteri olduğunda
            // Supabase'den kullanıcıları toplu olarak getiren bir yöntem düşünülmelidir.
            var tasks = customerDtos.Select(async (dto) =>
            {
                var customer = customers.First(c => c.Id == dto.Id);
                if (customer.ApplicationUserId.HasValue)
                {
                    var supabaseUser = await _supabaseService.GetUserById(customer.ApplicationUserId.Value.ToString(), cancellationToken);
                    dto.IsAccountActive = supabaseUser?.EmailConfirmedAt.HasValue ?? false;
                }
                else
                {
                    dto.IsAccountActive = false;
                }
            });

            await Task.WhenAll(tasks);
            
            return customerDtos;
        }
    }
}
