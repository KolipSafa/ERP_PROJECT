using Core.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Application.Features.Products.Commands;

namespace Application.Features.Products.Commands
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var productToDelete = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id);

            if (productToDelete == null)
            {
                throw new ValidationException($"ID'si {request.Id} olan ürün bulunamadı.");
            }

            _unitOfWork.ProductRepository.Delete(productToDelete);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
