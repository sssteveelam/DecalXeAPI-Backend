// DecalXeAPI/Services/Interfaces/IVehicleModelService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IVehicleModelService
    {
        Task<IEnumerable<VehicleModelDto>> GetAllModelsAsync();
        Task<VehicleModelDto?> GetModelByIdAsync(string id);
        Task<(VehicleModelDto?, string?)> CreateModelAsync(VehicleModel model);
        Task<(bool, string?)> UpdateModelAsync(string id, VehicleModel model);
        Task<bool> DeleteModelAsync(string id);
    }
}