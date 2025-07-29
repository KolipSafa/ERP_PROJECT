using Application.Common.Exceptions;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Teklifler.Commands
{
    /// <summary>
    /// Arşivlenmiş bir teklifi geri yüklemek (aktif hale getirmek) için kullanılan CQRS komutu.
    /// </summary>
    public class RestoreTeklifCommand : IRequest<Unit>
    {
        public Guid Id { get; }

        public RestoreTeklifCommand(Guid id)
        {
            Id = id;
        }
    }

    public class RestoreTeklifCommandHandler : IRequestHandler<RestoreTeklifCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RestoreTeklifCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(RestoreTeklifCommand request, CancellationToken cancellationToken)
        {
            // Geri yüklenecek teklifi bulurken, pasif olanları da dahil etmeliyiz.
            // Bu nedenle GetByIdAsync'in bu senaryoyu desteklediğinden emin olmalıyız.
            // Şimdilik, GetByIdAsync'in hem aktif hem pasif getirdiğini varsayıyoruz.
            var teklifToRestore = await _unitOfWork.TeklifRepository.GetByIdAsync(request.Id);

            if (teklifToRestore == null)
            {
                throw new NotFoundException(nameof(Teklif), request.Id);
            }

            if (teklifToRestore.IsActive)
            {
                // Zaten aktif olan bir teklifi tekrar restore etmeye çalışmayı önleyebiliriz.
                // Şimdilik bir hata fırlatmak yerine işlemi sessizce geçebiliriz.
                return Unit.Value;
            }

            teklifToRestore.IsActive = true;

            // Değişiklik izleyici bu değişikliği otomatik algılayacaktır.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
