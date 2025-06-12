using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IDesignCommentService
    {
        Task<IEnumerable<DesignCommentDto>> GetDesignCommentsAsync();
        Task<DesignCommentDto?> GetDesignCommentByIdAsync(string id);
        Task<DesignCommentDto> CreateDesignCommentAsync(DesignComment designComment);
        Task<bool> UpdateDesignCommentAsync(string id, DesignComment designComment);
        Task<bool> DeleteDesignCommentAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> DesignCommentExistsAsync(string id);
        Task<bool> DesignExistsAsync(string id); // Cần để kiểm tra FK
        Task<bool> AccountExistsAsync(string id); // Cần để kiểm tra FK (SenderAccountID)
    }
}