using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionDto>> GetPromotionsAsync();
        Task<PromotionDto?> GetPromotionByIdAsync(string id);
        Task<PromotionDto> CreatePromotionAsync(Promotion promotion);
        Task<bool> UpdatePromotionAsync(string id, Promotion promotion);
        Task<bool> DeletePromotionAsync(string id);

        // Hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> PromotionExistsAsync(string id);
    }
}