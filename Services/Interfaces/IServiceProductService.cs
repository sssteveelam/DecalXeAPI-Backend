using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IServiceProductService
    {
        Task<IEnumerable<ServiceProductDto>> GetServiceProductsAsync();
        Task<ServiceProductDto?> GetServiceProductByIdAsync(string id);
        Task<ServiceProductDto> CreateServiceProductAsync(ServiceProduct serviceProduct);
        Task<bool> UpdateServiceProductAsync(string id, ServiceProduct serviceProduct);
        Task<bool> DeleteServiceProductAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> ServiceProductExistsAsync(string id);
        Task<bool> DecalServiceExistsAsync(string id); // Cần để kiểm tra FK
        Task<bool> ProductExistsAsync(string id); // Cần để kiểm tra FK
    }
}