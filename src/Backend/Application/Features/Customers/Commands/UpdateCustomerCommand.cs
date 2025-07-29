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
    public class UpdateCustomerCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Guid? CompanyId { get; set; }
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
            customer.CompanyId = request.CompanyId ?? customer.CompanyId;
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