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

        Task<bool> WarrantyExistsAsync(string id);
        Task<bool> CustomerVehicleExistsAsync(string id); // Giữ lại
        // Task<bool> OrderExistsAsync(string id); // <-- ĐÃ XÓA DÒNG NÀY!
    }
}