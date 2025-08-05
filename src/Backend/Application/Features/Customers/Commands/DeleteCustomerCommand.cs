using Application.Common.Exceptions;
using Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Customers.Commands
{
    public record DeleteCustomerCommand(Guid Id) : IRequest<Unit>;

    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISupabaseAuthAdminService _supabaseAuthAdminService;
        private readonly ILogger<DeleteCustomerCommandHandler> _logger;

        public DeleteCustomerCommandHandler(
            IUnitOfWork unitOfWork, 
            ISupabaseAuthAdminService supabaseAuthAdminService,
            ILogger<DeleteCustomerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _supabaseAuthAdminService = supabaseAuthAdminService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(request.Id);

            if (customer == null)
            {
                throw new NotFoundException(nameof(Customer), request.Id);
            }

            // Eğer müşterinin bir kimlik doğrulama kaydı varsa, önce onu Supabase'den sil.
            if (customer.ApplicationUserId.HasValue && customer.ApplicationUserId.Value != Guid.Empty)
            {
                await _supabaseAuthAdminService.DeleteUser(customer.ApplicationUserId.Value.ToString(), cancellationToken);
            }
            else
            {
                _logger.LogWarning("Customer with ID {CustomerId} does not have a corresponding ApplicationUserId. Skipping Supabase user deletion.", request.Id);
            }

            // Supabase'den silme işlemi başarılı olsun veya olmasın (belki zaten silinmiştir),
            // yerel veritabanından kaydı sil.
            _unitOfWork.CustomerRepository.Delete(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted customer with ID {CustomerId} from the local database.", request.Id);

            return Unit.Value;
        }
    }
}
