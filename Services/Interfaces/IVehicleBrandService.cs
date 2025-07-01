// DecalXeAPI/Services/Interfaces/IVehicleBrandService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IVehicleBrandService
    {
        Task<IEnumerable<VehicleBrandDto>> GetAllBrandsAsync();
        Task<VehicleBrandDto?> GetBrandByIdAsync(string id);
        Task<VehicleBrandDto> CreateBrandAsync(VehicleBrand brand);
        Task<bool> UpdateBrandAsync(string id, VehicleBrand brand);
        Task<bool> DeleteBrandAsync(string id);
    }
}