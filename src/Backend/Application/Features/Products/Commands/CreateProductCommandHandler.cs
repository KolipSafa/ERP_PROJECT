using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Products.Commands
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name!,
                Description = request.Description,
                Price = request.Price!.Value,
                StockQuantity = request.StockQuantity!.Value,
                CurrencyId = request.CurrencyId!.Value,
                IsActive = true
            };

            product.SKU = await GenerateUniqueSku(request.Name!, cancellationToken);

            _unitOfWork.ProductRepository.Add(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<ProductDto>(product);
        }

        private async Task<string> GenerateUniqueSku(string productName, CancellationToken cancellationToken)
        {
            // Önce temizlenmiş SKU'yu bir değişkene alıyoruz.
            var cleanedSkuPart = Regex.Replace(productName.ToUpper(), @"[^A-Z0-9]", "");
            
            // Substring'i, temizlenmiş parçanın kendi uzunluğuna göre alıyoruz.
            var baseSku = cleanedSkuPart.Substring(0, Math.Min(cleanedSkuPart.Length, 10));

            var random = new Random();
            string potentialSku;

            do
            {
                var randomSuffix = random.Next(1000, 9999);
                potentialSku = $"{baseSku}-{randomSuffix}";
            }
            while (await _unitOfWork.ProductRepository.GetBySkuAsync(potentialSku) != null);

            return potentialSku;
        }
    }
}
