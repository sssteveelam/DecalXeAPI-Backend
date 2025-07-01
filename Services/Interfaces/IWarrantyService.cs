// DecalXeAPI/Services/Interfaces/IWarrantyService.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IWarrantyService
    {
        Task<IEnumerable<WarrantyDto>> GetWarrantiesAsync();
        Task<WarrantyDto?> GetWarrantyByIdAsync(string id);
        Task<WarrantyDto> CreateWarrantyAsync(Warranty warranty);
        Task<bool> UpdateWarrantyAsync(string id, Warranty warranty);
        Task<bool> DeleteWarrantyAsync(string id);

        // Các hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> WarrantyExistsAsync(string id);
        // Bỏ OrderExistsAsync, thay bằng CustomerVehicleExistsAsync
        Task<bool> CustomerVehicleExistsAsync(string vehicleId); 
    }
}