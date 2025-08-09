using Application.Common.Exceptions;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Teklifler.Commands
{
    public record HardDeleteTeklifCommand(Guid Id) : IRequest<Unit>;

    public class HardDeleteTeklifCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<HardDeleteTeklifCommand, Unit>
    {
        public async Task<Unit> Handle(HardDeleteTeklifCommand request, CancellationToken cancellationToken)
        {
            var teklif = await unitOfWork.TeklifRepository.GetByIdAsync(request.Id);
            if (teklif == null)
                throw new NotFoundException(nameof(Teklif), request.Id);

            // Rezerve serbest (ihtiyaten)
            foreach (var satir in teklif.TeklifSatirlari)
            {
                var product = await unitOfWork.ProductRepository.GetByIdAsync(satir.UrunId);
                if (product != null)
                {
                    product.ReservedQuantity = Math.Max(0, product.ReservedQuantity - (int)satir.Miktar);
                    unitOfWork.ProductRepository.Update(product);
                }
            }

            unitOfWork.TeklifRepository.HardDelete(teklif);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}


