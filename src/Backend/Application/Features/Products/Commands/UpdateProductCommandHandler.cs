using AutoMapper;
using Application.DTOs;
using Core.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Application.Features.Products.Commands;

namespace Application.Features.Products.Commands
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var productToUpdate = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id);
            if (productToUpdate == null)
            {
                throw new ValidationException($"ID'si {request.Id} olan ürün bulunamadı.");
            }

            // Akıllı güncelleme
            if (request.Name != null)
            {
                productToUpdate.Name = request.Name;
            }
            if (request.Description != null)
            {
                productToUpdate.Description = request.Description;
            }
            if (request.Price.HasValue)
            {
                productToUpdate.Price = request.Price.Value;
            }
            if (request.CurrencyId.HasValue)
            {
                productToUpdate.CurrencyId = request.CurrencyId.Value;
            }
            if (request.StockQuantity.HasValue)
            {
                productToUpdate.StockQuantity = request.StockQuantity.Value;
            }
            if (request.SKU != null)
            {
                // SKU'nun benzersiz olup olmadığını kontrol et (eğer değiştiyse)
                if (productToUpdate.SKU != request.SKU)
                {
                    var existingProduct = await _unitOfWork.ProductRepository.GetBySkuAsync(request.SKU);
                    if (existingProduct != null && existingProduct.Id != request.Id)
                    {
                        throw new ValidationException($"Bu SKU ('{request.SKU}') zaten başka bir ürüne aittir.");
                    }
                    productToUpdate.SKU = request.SKU;
                }
            }
            if (request.IsActive.HasValue)
            {
                productToUpdate.IsActive = request.IsActive.Value;
            }

            _unitOfWork.ProductRepository.Update(productToUpdate);
            await _unitOfWork.SaveChangesAsync();
            
            return _mapper.Map<ProductDto>(productToUpdate);
        }
    }
}
