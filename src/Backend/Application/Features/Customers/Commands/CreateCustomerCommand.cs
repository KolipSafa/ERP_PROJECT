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
    public record CreateCustomerCommand : IRequest<CustomerDto>, ICustomerFirstName, ICustomerLastName, ICustomerEmail
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public decimal? Balance { get; set; }
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
            var newCustomer = new Customer
            {
                FirstName = request.FirstName!,
                LastName = request.LastName!,
                CompanyName = request.CompanyName,
                TaxNumber = request.TaxNumber,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Balance = request.Balance ?? 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _unitOfWork.CustomerRepository.Add(newCustomer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CustomerDto>(newCustomer);
        }
    }
}