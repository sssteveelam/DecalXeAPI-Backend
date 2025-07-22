// DecalXeAPI/Services/Interfaces/ICustomerVehicleService.cs
using DecalXeAPI.DTOs;

namespace DecalXeAPI.Services.Interfaces
{
    public interface ICustomerVehicleService
    {
        Task<IEnumerable<CustomerVehicleDto>> GetAllAsync();
        Task<CustomerVehicleDto?> GetByIdAsync(string id);
        Task<CustomerVehicleDto?> GetByLicensePlateAsync(string licensePlate);
        Task<IEnumerable<CustomerVehicleDto>> GetByCustomerIdAsync(string customerId);
        Task<CustomerVehicleDto> CreateAsync(CreateCustomerVehicleDto createDto);
        Task<CustomerVehicleDto?> UpdateAsync(string id, UpdateCustomerVehicleDto updateDto);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> LicensePlateExistsAsync(string licensePlate);
        Task<bool> ChassisNumberExistsAsync(string chassisNumber);
    }
}
