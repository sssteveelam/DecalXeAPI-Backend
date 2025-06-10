using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(string id);
        Task<ProductDto> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(string id, Product product);
        Task<bool> DeleteProductAsync(string id);
        Task<bool> ProductExistsAsync(string id);
    }
}