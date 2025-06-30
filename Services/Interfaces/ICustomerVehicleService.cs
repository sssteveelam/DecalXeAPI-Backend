using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface ICustomerVehicleService
    {
        Task<IEnumerable<CustomerVehicleDto>> GetCustomerVehiclesAsync();
        Task<CustomerVehicleDto?> GetCustomerVehicleByIdAsync(string id);
        Task<CustomerVehicleDto> CreateCustomerVehicleAsync(CustomerVehicle customerVehicle);
        Task<bool> UpdateCustomerVehicleAsync(string id, CustomerVehicle customerVehicle);
        Task<bool> DeleteCustomerVehicleAsync(string id);

        Task<bool> CustomerVehicleExistsAsync(string id);
        Task<bool> CustomerExistsAsync(string id);
        Task<bool> VehicleModelExistsAsync(string id); // Kiểm tra ModelID của VehicleModel
    }
}