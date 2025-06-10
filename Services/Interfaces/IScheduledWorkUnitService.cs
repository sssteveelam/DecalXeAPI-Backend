using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IScheduledWorkUnitService
    {
        Task<IEnumerable<ScheduledWorkUnitDto>> GetScheduledWorkUnitsAsync();
        Task<ScheduledWorkUnitDto?> GetScheduledWorkUnitByIdAsync(string id);
        Task<(ScheduledWorkUnitDto? ScheduledWorkUnit, string? ErrorMessage)> CreateScheduledWorkUnitAsync(ScheduledWorkUnit scheduledWorkUnit);
        Task<(bool Success, string? ErrorMessage)> UpdateScheduledWorkUnitAsync(string id, ScheduledWorkUnit scheduledWorkUnit);
        Task<(bool Success, string? ErrorMessage)> DeleteScheduledWorkUnitAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service
        Task<bool> ScheduledWorkUnitExistsAsync(string id);
        Task<bool> TechnicianDailyScheduleExistsAsync(string id);
        Task<bool> TimeSlotDefinitionExistsAsync(string id);
        Task<bool> OrderExistsAsync(string id);
    }
}