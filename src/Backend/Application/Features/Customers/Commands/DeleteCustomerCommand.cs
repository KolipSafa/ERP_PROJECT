// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\Application\Features\Customers\Commands\DeleteCustomerCommand.cs

using Application.Common.Exceptions;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Customers.Commands
{
    public record DeleteCustomerCommand(Guid Id) : IRequest<Unit>;

    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCustomerCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(request.Id);

            if (customer == null)
            {
                throw new NotFoundException(nameof(Customer), request.Id);
            }

            _unitOfWork.CustomerRepository.Delete(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
