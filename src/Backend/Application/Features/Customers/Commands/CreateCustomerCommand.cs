// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\Application\Features\Customers\Commands\CreateCustomerCommand.cs
using Application.DTOs;
using Application.Validators.Customers;
using MediatR;
using Core.Domain.Interfaces;
using AutoMapper;
using Core.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Commands
{
    public class CreateCustomerCommand : IRequest<CustomerDto>, ICustomerFirstName, ICustomerLastName, ICustomerEmail
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Guid CompanyId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public decimal Balance { get; set; }
    }

    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new Customer
            {
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                CompanyId = request.CompanyId,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Balance = request.Balance,
                CreatedDate = DateTime.UtcNow
            };

            _unitOfWork.CustomerRepository.Add(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Not: Yeni oluşturulan müşteriyi DTO olarak döndürmek için tekrar okumak en doğrusu.
            // Şimdilik basitlik adına manuel map'leme yapabiliriz veya AutoMapper kullanabiliriz.
            // En sağlıklısı GetByIdAsync ile çekmektir.
            var createdCustomer = await _unitOfWork.CustomerRepository.GetByIdAsync(customer.Id);

            return _mapper.Map<CustomerDto>(createdCustomer);
        }
    }
}