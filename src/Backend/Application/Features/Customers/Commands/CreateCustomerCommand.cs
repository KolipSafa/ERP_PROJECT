using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Exceptions;
using FluentValidation;
using Application.Interfaces;
using System.Collections.Generic;

namespace Application.Features.Customers.Commands
{
    public class CreateCustomerCommand : IRequest<ActivationResponseDto>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Guid CompanyId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public Guid ApplicationUserId { get; set; }
    }

    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, ActivationResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISupabaseAuthAdminService _supabaseAuthAdminService;

        public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ISupabaseAuthAdminService supabaseAuthAdminService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _supabaseAuthAdminService = supabaseAuthAdminService;
        }

        public async Task<ActivationResponseDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var existingCustomer = await _unitOfWork.CustomerRepository.GetByEmailAsync(request.Email);
            if (existingCustomer != null)
            {
                throw new ValidationException($"'{request.Email}' e-posta adresi zaten sistemde kayıtlı.");
            }

            var customer = new Customer
            {
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                CompanyId = request.CompanyId,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                ApplicationUserId = request.ApplicationUserId,
                CreatedDate = DateTime.UtcNow
            };

            _unitOfWork.CustomerRepository.Add(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedSupabaseUser = await _supabaseAuthAdminService.UpdateUserAppMetadata(
                request.ApplicationUserId.ToString(),
                new Dictionary<string, object> { { "status", "active" } }
            );

            var createdCustomer = await _unitOfWork.CustomerRepository.GetByIdAsync(customer.Id);
            
            return new ActivationResponseDto
            {
                Customer = _mapper.Map<CustomerDto>(createdCustomer),
                SupabaseUser = updatedSupabaseUser
            };
        }
    }
}
