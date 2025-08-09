using Application.Common.Exceptions;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using MediatR;

namespace Application.Features.Teklifler.Commands
{
    public record ResendTeklifCommand(Guid Id) : IRequest<Unit>;

    public class ResendTeklifCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ResendTeklifCommand, Unit>
    {
        public async Task<Unit> Handle(ResendTeklifCommand request, CancellationToken cancellationToken)
        {
            var teklif = await unitOfWork.TeklifRepository.GetByIdAsync(request.Id);
            if (teklif == null)
                throw new NotFoundException(nameof(Teklif), request.Id);

            // Yalnızca durumu ChangeRequested veya Sunuldu olmayanlar için tekrar sunmak mantıklı olabilir,
            // ancak burada doğrudan Sunuldu'ya çekiyoruz.
            teklif.Durum = QuoteStatus.Sunuldu;
            teklif.ChangeRequestNotes = null;
            teklif.IsActive = true;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}


