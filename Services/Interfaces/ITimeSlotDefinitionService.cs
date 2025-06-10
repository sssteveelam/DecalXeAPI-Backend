using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface ITimeSlotDefinitionService
    {
        Task<IEnumerable<TimeSlotDefinitionDto>> GetTimeSlotDefinitionsAsync();
        Task<TimeSlotDefinitionDto?> GetTimeSlotDefinitionByIdAsync(string id);
        Task<TimeSlotDefinitionDto> CreateTimeSlotDefinitionAsync(TimeSlotDefinition timeSlotDefinition);
        Task<bool> UpdateTimeSlotDefinitionAsync(string id, TimeSlotDefinition timeSlotDefinition);
        Task<bool> DeleteTimeSlotDefinitionAsync(string id);

        // Hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> TimeSlotDefinitionExistsAsync(string id);
    }
}