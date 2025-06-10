using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Interfaces
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreDto>> GetStoresAsync();
        Task<StoreDto?> GetStoreByIdAsync(string id);
        Task<StoreDto> CreateStoreAsync(Store store);
        Task<bool> UpdateStoreAsync(string id, Store store);
        Task<bool> DeleteStoreAsync(string id);

        // Hàm kiểm tra tồn tại (Exists) cần thiết cho Service này
        Task<bool> StoreExistsAsync(string id);
    }
}