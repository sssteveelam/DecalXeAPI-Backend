// DecalXeAPI/Services/Interfaces/IServiceVehicleModelProductService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IServiceVehicleModelProductService
    {
        Task<List<ServiceVehicleModelProductDto>> GetAllAsync();
        Task<ServiceVehicleModelProductDto?> GetByIdAsync(string serviceId, string vehicleModelId, string productId);
        Task<ServiceVehicleModelProductDto> CreateAsync(ServiceVehicleModelProduct link);
        Task<ServiceVehicleModelProductDto?> UpdateQuantityAsync(string serviceId, string vehicleModelId, string productId, int newQuantity);
        Task<bool> DeleteAsync(string serviceId, string vehicleModelId, string productId);
    }
}