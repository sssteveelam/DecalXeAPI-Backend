using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<IEnumerable<FeedbackDto>> GetFeedbacksAsync();
        Task<FeedbackDto?> GetFeedbackByIdAsync(string id);
        Task<FeedbackDto> CreateFeedbackAsync(Feedback feedback);
        Task<bool> UpdateFeedbackAsync(string id, Feedback feedback);
        Task<bool> DeleteFeedbackAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> FeedbackExistsAsync(string id);
        Task<bool> OrderExistsAsync(string id); // Cần để kiểm tra FK
        Task<bool> CustomerExistsAsync(string id); // Cần để kiểm tra FK
    }
}