using Application.Validators.Customers;
using MediatR;
using System;
using Core.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Exceptions;
using Core.Domain.Entities;
using AutoMapper;

namespace Application.Features.Customers.Commands
{
    public record UpdateCustomerCommand : IRequest<Unit>, ICustomerFirstName, ICustomerLastName, ICustomerEmail
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public decimal? Balance { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(request.Id);

            if (customer == null)
            {
                throw new NotFoundException(nameof(Customer), request.Id);
            }

            customer.FirstName = request.FirstName ?? customer.FirstName;
            customer.LastName = request.LastName ?? customer.LastName;
            customer.CompanyName = request.CompanyName ?? customer.CompanyName;
            customer.TaxNumber = request.TaxNumber ?? customer.TaxNumber;
            customer.Address = request.Address ?? customer.Address;
            customer.PhoneNumber = request.PhoneNumber ?? customer.PhoneNumber;
            customer.Email = request.Email ?? customer.Email;
            customer.Balance = request.Balance ?? customer.Balance;
            customer.IsActive = request.IsActive ?? customer.IsActive;
            customer.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.CustomerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}