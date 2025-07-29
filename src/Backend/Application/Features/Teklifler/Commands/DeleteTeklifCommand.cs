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
    /// Bir teklifi arşivlemek (soft delete) için kullanılan CQRS komutu.
    /// </summary>
    public class DeleteTeklifCommand : IRequest<Unit>
    {
        public Guid Id { get; }

        public DeleteTeklifCommand(Guid id)
        {
            Id = id;
        }
    }

    public class DeleteTeklifCommandHandler : IRequestHandler<DeleteTeklifCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTeklifCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteTeklifCommand request, CancellationToken cancellationToken)
        {
            var teklifToDelete = await _unitOfWork.TeklifRepository.GetByIdAsync(request.Id);

            if (teklifToDelete == null)
            {
                throw new NotFoundException(nameof(Teklif), request.Id);
            }

            teklifToDelete.IsActive = false;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
