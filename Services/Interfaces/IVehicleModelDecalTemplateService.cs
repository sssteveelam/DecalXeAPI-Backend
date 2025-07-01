// DecalXeAPI/Services/Interfaces/IVehicleModelDecalTemplateService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IVehicleModelDecalTemplateService
    {
        Task<IEnumerable<VehicleModelDecalTemplateDto>> GetAllAsync();
        Task<(VehicleModelDecalTemplateDto?, string?)> CreateAsync(VehicleModelDecalTemplate link);
        Task<bool> DeleteAsync(string id);
    }
}