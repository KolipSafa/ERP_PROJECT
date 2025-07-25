using Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync(
            bool includeInactive = false,
            string? search = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = null,
            string? sortOrder = null
        );
        void Add(Product product);
        void Update(Product product);
        void Delete(Product product);
        void HardDelete(Product product);
        Task<Product?> GetBySkuAsync(string sku);
    }
}
