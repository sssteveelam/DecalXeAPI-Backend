using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface ITechnicianDailyScheduleService
    {
        Task<IEnumerable<TechnicianDailyScheduleDto>> GetTechnicianDailySchedulesAsync();
        Task<TechnicianDailyScheduleDto?> GetTechnicianDailyScheduleByIdAsync(string id);
        Task<TechnicianDailyScheduleDto> CreateTechnicianDailyScheduleAsync(TechnicianDailySchedule schedule);
        Task<bool> UpdateTechnicianDailyScheduleAsync(string id, TechnicianDailySchedule schedule);
        Task<bool> DeleteTechnicianDailyScheduleAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> TechnicianDailyScheduleExistsAsync(string id);
        Task<bool> EmployeeExistsAsync(string id); // Cần để kiểm tra FK
    }
}