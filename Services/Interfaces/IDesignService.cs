// DecalXeAPI/Services/Interfaces/IDesignService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IDesignService
    {
        Task<IEnumerable<DesignDto>> GetDesignsAsync();
        Task<DesignDto?> GetDesignByIdAsync(string id);
        Task<DesignDto> CreateDesignAsync(Design design);
        Task<bool> UpdateDesignAsync(string id, Design design);
        Task<bool> DeleteDesignAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> DesignExistsAsync(string id);
        // Task<bool> OrderExistsAsync(string id); // <-- XÓA DÒNG NÀY ĐI
        Task<bool> EmployeeExistsAsync(string id); // Cần để kiểm tra FK (DesignerID)
    }
}