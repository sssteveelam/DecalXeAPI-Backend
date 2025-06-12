using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IOrderCompletionImageService
    {
        Task<IEnumerable<OrderCompletionImageDto>> GetOrderCompletionImagesAsync();
        Task<OrderCompletionImageDto?> GetOrderCompletionImageByIdAsync(string id);
        Task<OrderCompletionImageDto> CreateOrderCompletionImageAsync(OrderCompletionImage image);
        Task<bool> UpdateOrderCompletionImageAsync(string id, OrderCompletionImage image);
        Task<bool> DeleteOrderCompletionImageAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> OrderCompletionImageExistsAsync(string id);
        Task<bool> OrderExistsAsync(string id); // Cần để kiểm tra FK
    }
}