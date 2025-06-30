using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IVehicleService
    {
        // API cho VehicleBrand
        Task<IEnumerable<VehicleBrandDto>> GetVehicleBrandsAsync();
        Task<VehicleBrandDto?> GetVehicleBrandByIdAsync(string id);
        Task<VehicleBrandDto> CreateVehicleBrandAsync(VehicleBrand brand);
        Task<bool> UpdateVehicleBrandAsync(string id, VehicleBrand brand);
        Task<bool> DeleteVehicleBrandAsync(string id);
        Task<bool> VehicleBrandExistsAsync(string id);

        // API cho VehicleModel
        Task<IEnumerable<VehicleModelDto>> GetVehicleModelsAsync();
        Task<VehicleModelDto?> GetVehicleModelByIdAsync(string id);
        Task<VehicleModelDto> CreateVehicleModelAsync(VehicleModel model);
        Task<bool> UpdateVehicleModelAsync(string id, VehicleModel model);
        Task<bool> DeleteVehicleModelAsync(string id);
        Task<bool> VehicleModelExistsAsync(string id);

        // API cho VehicleModelDecalTemplate
        Task<IEnumerable<VehicleModelDecalTemplateDto>> GetVehicleModelDecalTemplatesAsync();
        Task<VehicleModelDecalTemplateDto?> GetVehicleModelDecalTemplateByIdAsync(string id);
        Task<VehicleModelDecalTemplateDto> CreateVehicleModelDecalTemplateAsync(VehicleModelDecalTemplate template);
        Task<bool> UpdateVehicleModelDecalTemplateAsync(string id, VehicleModelDecalTemplate template);
        Task<bool> DeleteVehicleModelDecalTemplateAsync(string id);
        Task<bool> VehicleModelDecalTemplateExistsAsync(string id);
    }
}