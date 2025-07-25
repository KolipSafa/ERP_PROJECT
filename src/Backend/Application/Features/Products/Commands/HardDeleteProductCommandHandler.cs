using Core.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Application.Features.Products.Commands;

namespace Application.Features.Products.Commands
{
    public class HardDeleteProductCommandHandler : IRequestHandler<HardDeleteProductCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public HardDeleteProductCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(HardDeleteProductCommand request, CancellationToken cancellationToken)
        {
            var productToDelete = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id);
            if (productToDelete == null)
            {
                // Opsiyonel: Silinmeye çalışılan ürün zaten yoksa hata fırlatmak yerine sessizce geçilebilir.
                // Ancak genellikle bir şeyin beklendiği gibi gitmediğini belirtmek daha iyidir.
                throw new ValidationException($"ID'si {request.Id} olan ürün bulunamadı.");
            }

            _unitOfWork.ProductRepository.HardDelete(productToDelete);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
